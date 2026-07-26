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

        return await ProjectStockAsync(query.OrderBy(p => p.Name), request, cancellationToken);
    }

    /// <summary>
    /// Shared stock projection. The last purchase cost is a correlated subquery so the page costs
    /// one round trip; the valuation is rounded once the rows are materialised.
    /// </summary>
    private async Task<PagedResult<StockLevelDto>> ProjectStockAsync(
        IQueryable<Product> products,
        PageRequest request,
        CancellationToken cancellationToken)
    {
        var page = await products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.PartNumber,
                TrademarkName = p.Trademark != null ? p.Trademark.Name : null,
                CategoryName = p.Category != null ? p.Category.Name : null,
                Quantity = p.StockLevel != null ? p.StockLevel.Quantity : 0,
                UpdatedAt = p.StockLevel != null ? p.StockLevel.UpdatedAt : p.CreatedAt,
                LastCost = db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Purchase && m.UnitCost != null)
                    // Ledger ids are monotonic: the highest id is the most recent purchase cost.
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.UnitCost)
                    .FirstOrDefault()
            })
            .ToPagedResultAsync(request, cancellationToken);

        var items = page.Items
            .Select(r => new StockLevelDto(
                r.Id,
                r.Name,
                r.PartNumber,
                r.TrademarkName,
                r.CategoryName,
                r.Quantity,
                r.LastCost,
                decimal.Round(r.Quantity * (r.LastCost ?? 0m), 2, MidpointRounding.AwayFromZero),
                r.UpdatedAt))
            .ToList();

        return new PagedResult<StockLevelDto>(items, page.Page, page.PageSize, page.Total);
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

            // Re-read through the shared projection so the caller gets the same shape as the list,
            // revalued at the product's last purchase cost.
            var page = await ProjectStockAsync(
                db.Products.AsNoTracking().Where(p => p.Id == product.Id),
                new PageRequest(null, 1, 1),
                ct);

            return page.Items[0];
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
                p.PartNumber,
                p.Trademark != null ? p.Trademark.Name : null,
                db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Purchase)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0,
                -(db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Sale)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0),
                db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Adjustment)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0,
                // Through Sales so the soft-delete filter applies; SaleItem carries none of its own.
                db.Sales
                    .SelectMany(s => s.Items)
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (decimal?)i.Subtotal) ?? 0m,
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
