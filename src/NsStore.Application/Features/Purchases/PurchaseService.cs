using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Branches;
using NsStore.Application.Features.Inventory;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Purchases;

public class PurchaseService(
    IAppDbContext db,
    InventoryService inventory,
    SerialService serials,
    BranchService branches,
    IStockLockService stockLock,
    IDocumentNumberService documentNumbers,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task<PagedResult<PurchaseListItemDto>> ListAsync(PurchaseQuery query, CancellationToken cancellationToken = default)
    {
        // Same policy as sales: a seller sees only their own branch's buying.
        var branchId = currentUser.ResolveScopedBranch(query.BranchId);

        var request = new PageRequest(query.Search, query.Page, query.PageSize);
        var purchases = db.Purchases.AsNoTracking().AsQueryable();

        if (branchId is { } scoped)
        {
            purchases = purchases.Where(p => p.BranchId == scoped);
        }

        if (request.SearchPattern is { } pattern)
        {
            purchases = purchases.Where(p => EF.Functions.Like(SearchText.Unaccent(p.Supplier.Name).ToLower(), pattern));
        }

        if (query.From is { } from)
        {
            purchases = purchases.Where(p => p.PurchaseDate >= from);
        }

        if (query.To is { } to)
        {
            purchases = purchases.Where(p => p.PurchaseDate <= to);
        }

        return await purchases
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.Id)
            .Select(p => new PurchaseListItemDto(
                p.Id,
                p.PurchaseDate,
                p.BranchId,
                p.Branch.Code,
                p.Number,
                p.SupplierId,
                p.Supplier.Name,
                p.InvoiceType,
                p.PaymentStatus,
                p.Items.Count,
                p.TotalQuantity,
                p.TotalAmount,
                db.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Username).FirstOrDefault()))
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<PurchaseDto> GetAsync(long id, CancellationToken cancellationToken = default) =>
        await db.Purchases.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PurchaseDto(
                p.Id,
                p.PurchaseDate,
                p.BranchId,
                p.Branch.Code,
                p.Number,
                p.SupplierId,
                p.Supplier.Name,
                p.InvoiceType,
                p.PaymentStatus,
                p.TotalQuantity,
                p.TotalAmount,
                p.CreatedBy,
                db.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Username).FirstOrDefault(),
                p.CreatedAt,
                p.Items.Select(i => new PurchaseItemDto(
                    i.Id,
                    i.ProductId,
                    i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    i.Subtotal,
                    i.Serials.OrderBy(s => s.SerialNumber).Select(s => s.SerialNumber).ToList())).ToList()))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Purchase), id);

    /// <summary>
    /// Header + lines + ledger movements + stock increments, all in one transaction.
    /// Totals are computed server-side; the client's totals are ignored.
    /// </summary>
    public async Task<PurchaseDto> CreateAsync(CreatePurchaseRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new BadRequestException("A purchase requires at least one item");
        }

        var branchId = currentUser.RequireWritableBranch();

        var purchaseId = await db.ExecuteInTransactionAsync(async ct =>
        {
            await branches.EnsureWritableAsync(branchId, ct);

            if (!await db.Suppliers.AnyAsync(s => s.Id == request.SupplierId, ct))
            {
                throw new NotFoundException(nameof(Supplier), request.SupplierId);
            }

            // Merge duplicate lines for the same product so one product locks/moves stock once.
            var lines = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Items = g.ToList() })
                .ToList();

            var productIds = lines.Select(l => l.ProductId).ToList();
            await stockLock.LockAsync([.. productIds.Select(id => new StockKey(branchId, id))], ct);

            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            var missing = productIds.FirstOrDefault(id => !products.ContainsKey(id));
            if (missing != 0)
            {
                throw new NotFoundException(nameof(Product), missing);
            }

            var now = clock.GetUtcNow();

            // Counter after the stock locks; see SaleService for the ordering rule.
            var branch = await db.Branches.FirstAsync(b => b.Id == branchId, ct);
            var sequence = await documentNumbers.NextAsync(branchId, DocumentKind.Purchase, ct);

            var purchase = new Purchase
            {
                PurchaseDate = request.PurchaseDate,
                BranchId = branchId,
                BranchSequence = sequence,
                Number = branch.FormatDocumentNumber(sequence),
                SupplierId = request.SupplierId,
                InvoiceType = request.InvoiceType,
                PaymentStatus = request.PaymentStatus
            };

            // Across the whole document, not per line: two lines of the same product must not both
            // claim one serial.
            SerialService.NormalizeBatch([.. request.Items.SelectMany(i => i.SerialNumbers ?? [])]);

            foreach (var item in request.Items)
            {
                var subtotal = decimal.Round(item.UnitPrice * item.Quantity, 2, MidpointRounding.AwayFromZero);
                purchase.Items.Add(new PurchaseItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = decimal.Round(item.UnitPrice, 2, MidpointRounding.AwayFromZero),
                    Subtotal = subtotal
                });
            }

            purchase.TotalQuantity = purchase.Items.Sum(i => i.Quantity);
            purchase.TotalAmount = purchase.Items.Sum(i => i.Subtotal);
            db.Purchases.Add(purchase);
            await db.SaveChangesAsync(ct);

            foreach (var line in lines)
            {
                var quantity = line.Items.Sum(i => i.Quantity);
                var stock = await inventory.GetOrCreateStockLevelAsync(branchId, line.ProductId, now, ct);
                stock.Apply(quantity, now);

                // One movement per input line keeps the per-unit cost that feeds price suggestions.
                foreach (var item in line.Items)
                {
                    db.InventoryMovements.Add(new InventoryMovement
                    {
                        BranchId = branchId,
                        ProductId = line.ProductId,
                        MovementType = MovementType.Purchase,
                        QuantityDelta = item.Quantity,
                        UnitCost = decimal.Round(item.UnitPrice, 2, MidpointRounding.AwayFromZero),
                        ReferenceType = "purchase",
                        ReferenceId = purchase.Id
                    });
                }
            }

            // Purchase.Items was built from request.Items in order, so the two line up. Attaching by
            // navigation lets EF fix up the FK — no second SaveChanges to fish ids out.
            foreach (var (item, line) in request.Items.Zip(purchase.Items))
            {
                var created = await serials.CreateInboundAsync(
                    branchId, products[item.ProductId], item.Quantity, item.SerialNumbers, line.Id, now, ct);

                foreach (var serial in created)
                {
                    serials.RecordEvent(serial, SerialEventType.Received, branchId, "purchase", purchase.Id);
                }
            }

            await db.SaveChangesAsync(ct);
            return purchase.Id;
        }, cancellationToken);

        return await GetAsync(purchaseId, cancellationToken);
    }
}
