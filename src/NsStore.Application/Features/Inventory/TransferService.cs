using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Branches;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

/// <summary>
/// Moves stock between branches. Lives inside the inventory feature rather than a new one — it is
/// an inventory operation.
/// </summary>
public class TransferService(
    IAppDbContext db,
    InventoryService inventory,
    SerialService serials,
    BranchService branches,
    IStockLockService stockLock,
    IDocumentNumberService documentNumbers,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task<PagedResult<TransferListItemDto>> ListAsync(
        TransferQuery query,
        CancellationToken cancellationToken = default)
    {
        var request = query.ToPageRequest();
        var transfers = db.StockTransfers.AsNoTracking().AsQueryable();

        if (query.From is { } from)
        {
            transfers = transfers.Where(t => t.TransferDate >= from);
        }

        if (query.To is { } to)
        {
            transfers = transfers.Where(t => t.TransferDate <= to);
        }

        // A branch's list covers both what it sent and what it received.
        if (query.BranchId is { } branchId)
        {
            transfers = transfers.Where(t => t.OriginBranchId == branchId || t.DestinationBranchId == branchId);
        }

        return await transfers
            .OrderByDescending(t => t.TransferDate)
            .ThenByDescending(t => t.Id)
            .Select(t => new TransferListItemDto(
                t.Id,
                t.Number,
                t.TransferDate,
                t.OriginBranchId,
                t.OriginBranch.Code,
                t.DestinationBranchId,
                t.DestinationBranch.Code,
                t.Items.Count,
                t.TotalQuantity,
                db.Users.Where(u => u.Id == t.CreatedBy).Select(u => u.Username).FirstOrDefault()))
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<TransferDto> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var transfer = await db.StockTransfers.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                Transfer = t,
                OriginCode = t.OriginBranch.Code,
                DestinationCode = t.DestinationBranch.Code,
                CreatedByName = db.Users.Where(u => u.Id == t.CreatedBy).Select(u => u.Username).FirstOrDefault(),
                Items = t.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Product.PartNumber,
                    i.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(StockTransfer), id);

        // Read from the history rather than the units themselves: a unit only knows where it stands
        // now, so after a second hop it could no longer say it travelled on this note. Fetched
        // separately because nesting it inside the line projection needs SQL APPLY, which SQLite —
        // the test provider — does not have.
        var moved = (await db.ProductSerialEvents.AsNoTracking()
            .Where(e =>
                e.ReferenceType == "transfer" &&
                e.ReferenceId == id &&
                e.EventType == SerialEventType.TransferredOut)
            .Select(e => new { e.Serial.ProductId, e.Serial.SerialNumber })
            .ToListAsync(cancellationToken))
            .GroupBy(e => e.ProductId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)[.. g.Select(e => e.SerialNumber).Order()]);

        return new TransferDto(
            transfer.Transfer.Id,
            transfer.Transfer.Number,
            transfer.Transfer.TransferDate,
            transfer.Transfer.OriginBranchId,
            transfer.OriginCode,
            transfer.Transfer.DestinationBranchId,
            transfer.DestinationCode,
            transfer.Transfer.TotalQuantity,
            transfer.Transfer.Notes,
            transfer.Transfer.CreatedBy,
            transfer.CreatedByName,
            transfer.Transfer.CreatedAt,
            [.. transfer.Items.Select(i => new TransferItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.PartNumber,
                i.Quantity,
                moved.GetValueOrDefault(i.ProductId, [])))]);
    }

    /// <summary>
    /// One transaction: lock both sides, decrement the origin, increment the destination, write two
    /// ledger entries per product. If the origin lacks stock the whole thing rolls back and the
    /// destination is untouched — that atomicity is the heart of the design.
    /// </summary>
    public async Task<TransferDto> CreateAsync(CreateTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A transfer requires at least one item");
        }

        if (request.OriginBranchId == request.DestinationBranchId)
        {
            throw new DomainRuleException(
                ErrorCodes.SameBranchTransfer,
                "Origin and destination must be different branches");
        }

        // The origin is the writing side: you may only dispatch from a branch you can write to.
        var originBranchId = currentUser.RequireWritableBranch(request.OriginBranchId);
        var destinationBranchId = request.DestinationBranchId;

        var transferId = await db.ExecuteInTransactionAsync(async ct =>
        {
            await branches.EnsureWritableAsync(originBranchId, ct);
            await branches.EnsureWritableAsync(destinationBranchId, ct);

            // Same product on several lines moves once.
            var quantities = request.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            // One call with all four keys, not two calls: a single ordering rule is what stops an
            // A->B transfer deadlocking against a concurrent B->A.
            var keys = quantities.Keys
                .SelectMany(productId => new[]
                {
                    new StockKey(originBranchId, productId),
                    new StockKey(destinationBranchId, productId)
                })
                .ToArray();

            await stockLock.LockAsync(keys, ct);

            var productIds = quantities.Keys.ToList();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            var missing = productIds.FirstOrDefault(id => !products.ContainsKey(id));
            if (missing != 0)
            {
                throw new NotFoundException(nameof(Product), missing);
            }

            var now = clock.GetUtcNow();

            // Judged at the origin and per product, for the same reason as a sale.
            SerialService.NormalizeBatch([.. request.Items.SelectMany(i => i.SerialNumbers ?? [])]);

            var serialsByProduct = request.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)[.. g.SelectMany(i => i.SerialNumbers ?? [])]);

            var travelling = new Dictionary<long, List<ProductSerial>>();
            foreach (var (productId, quantity) in quantities)
            {
                travelling[productId] = [.. await serials.ResolveOutboundAsync(
                    originBranchId, products[productId], quantity, serialsByProduct.GetValueOrDefault(productId), ct)];
            }

            // Counter after the stock locks; see the ordering rule on IDocumentNumberService.
            var origin = await db.Branches.FirstAsync(b => b.Id == originBranchId, ct);
            var sequence = await documentNumbers.NextAsync(originBranchId, DocumentKind.Transfer, ct);

            var transfer = new StockTransfer
            {
                TransferDate = request.TransferDate,
                OriginBranchId = originBranchId,
                DestinationBranchId = destinationBranchId,
                BranchSequence = sequence,
                Number = origin.FormatDocumentNumber(sequence),
                Notes = request.Notes?.Trim(),
                TotalQuantity = quantities.Values.Sum()
            };

            foreach (var (productId, quantity) in quantities)
            {
                transfer.Items.Add(new StockTransferItem { ProductId = productId, Quantity = quantity });
            }

            db.StockTransfers.Add(transfer);
            await db.SaveChangesAsync(ct);

            foreach (var (productId, quantity) in quantities)
            {
                var originStock = await inventory.GetOrCreateStockLevelAsync(originBranchId, productId, now, ct);
                // Throws INSUFFICIENT_STOCK; the destination is never touched because the whole
                // action rolls back.
                originStock.Apply(-quantity, now);

                var destinationStock = await inventory.GetOrCreateStockLevelAsync(destinationBranchId, productId, now, ct);
                destinationStock.Apply(quantity, now);

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = originBranchId,
                    ProductId = productId,
                    MovementType = MovementType.TransferOut,
                    QuantityDelta = -quantity,
                    ReferenceType = "transfer",
                    ReferenceId = transfer.Id
                });

                db.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = destinationBranchId,
                    ProductId = productId,
                    MovementType = MovementType.TransferIn,
                    QuantityDelta = quantity,
                    ReferenceType = "transfer",
                    ReferenceId = transfer.Id
                });
            }

            // Two entries per unit, mirroring the two ledger movements: dispatched from one counter,
            // received at another, which is what the note on each side has to show.
            foreach (var serial in travelling.Values.SelectMany(s => s))
            {
                serials.RecordEvent(serial, SerialEventType.TransferredOut, originBranchId, "transfer", transfer.Id);
                serials.RecordEvent(serial, SerialEventType.TransferredIn, destinationBranchId, "transfer", transfer.Id);

                // The unit stays in stock; only where it stands changes.
                serial.MarkTransferred(destinationBranchId, now);
            }

            await db.SaveChangesAsync(ct);
            return transfer.Id;
        }, cancellationToken);

        return await GetAsync(transferId, cancellationToken);
    }
}
