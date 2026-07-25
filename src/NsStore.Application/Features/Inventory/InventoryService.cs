using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

public class InventoryService(IAppDbContext db, ICurrentUser currentUser, IStockLockService stockLock, TimeProvider clock)
{
    public async Task<PagedResult<StockLevelDto>> ListStockAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{search.ToLower()}%"));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new StockLevelDto(
                p.Id,
                p.Name,
                p.PartNumber,
                p.StockLevel != null ? p.StockLevel.Quantity : 0,
                p.StockLevel != null ? p.StockLevel.UpdatedAt : p.CreatedAt))
            .ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<PagedResult<InventoryMovementDto>> ListMovementsAsync(
        long productId,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        return await db.InventoryMovements.AsNoTracking()
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.Id)
            .Select(m => new InventoryMovementDto(
                m.Id,
                m.ProductId,
                m.Product.Name,
                m.MovementType,
                m.QuantityDelta,
                m.UnitCost,
                m.ReferenceType,
                m.ReferenceId,
                m.Notes,
                m.CreatedAt))
            .ToPagedResultAsync(request, cancellationToken);
    }

    /// <summary>Manual correction (admin): writes a ledger entry and moves the stock cache atomically.</summary>
    public async Task<StockLevelDto> AdjustAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.QuantityDelta == 0)
        {
            throw new BadRequestException("Quantity delta must not be zero");
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await stockLock.LockAsync([request.ProductId], ct);

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
                ?? throw new NotFoundException(nameof(Product), request.ProductId);

            var now = clock.GetUtcNow();
            var stock = await GetOrCreateStockLevelAsync(product.Id, now, ct);
            stock.Apply(request.QuantityDelta, now);

            db.InventoryMovements.Add(new InventoryMovement
            {
                ProductId = product.Id,
                MovementType = MovementType.Adjustment,
                QuantityDelta = request.QuantityDelta,
                ReferenceType = "manual",
                Notes = request.Notes?.Trim(),
                CreatedBy = currentUser.UserId,
                CreatedAt = now
            });

            await db.SaveChangesAsync(ct);
            return new StockLevelDto(product.Id, product.Name, product.PartNumber, stock.Quantity, stock.UpdatedAt);
        }, cancellationToken);
    }

    public async Task<PagedResult<KardexRowDto>> GetKardexAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{search.ToLower()}%"));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new KardexRowDto(
                p.Id,
                p.Name,
                db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Purchase)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0,
                -(db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Sale)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0),
                p.StockLevel != null ? p.StockLevel.Quantity : 0))
            .ToPagedResultAsync(request, cancellationToken);
    }

    /// <summary>
    /// Products created before this module existed (or seeded data) may lack a stock row;
    /// create it lazily so quantities always have a home.
    /// </summary>
    internal async Task<StockLevel> GetOrCreateStockLevelAsync(long productId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var stock = await db.StockLevels.FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
        if (stock is not null)
        {
            return stock;
        }

        stock = new StockLevel { ProductId = productId, Quantity = 0, UpdatedAt = now };
        db.StockLevels.Add(stock);
        return stock;
    }
}
