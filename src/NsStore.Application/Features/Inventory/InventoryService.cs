using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

public class InventoryService(IAppDbContext db, ICurrentUser currentUser, IStockLockService stockLock, TimeProvider clock)
{
    public async Task<PagedResult<StockLevelDto>> ListStockAsync(StockQuery query, CancellationToken cancellationToken = default)
    {
        var branchId = currentUser.ResolveReadableBranch(query.BranchId);
        var request = query.ToPageRequest();

        var products = db.Products.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            products = products.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{search.ToLower()}%"));
        }

        return await ProjectStockAsync(products.OrderBy(p => p.Name), branchId, request, cancellationToken);
    }

    /// <summary>
    /// Where a product sits across every active branch. No branch guard and no paging: the number
    /// of branches is small, and this is the read the cross-branch use case is built on.
    /// </summary>
    public async Task<IReadOnlyList<BranchAvailabilityDto>> GetAvailabilityAsync(
        long productId,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        return await db.Branches.AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Code)
            .Select(b => new BranchAvailabilityDto(
                b.Id,
                b.Code,
                b.Name,
                db.StockLevels
                    .Where(s => s.BranchId == b.Id && s.ProductId == productId)
                    .Sum(s => (int?)s.Quantity) ?? 0,
                db.StockLevels
                    .Where(s => s.BranchId == b.Id && s.ProductId == productId)
                    .Select(s => (DateTimeOffset?)s.UpdatedAt)
                    .FirstOrDefault() ?? b.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Shared stock projection, scoped to one branch. The last purchase cost stays global — the
    /// price list is global, so a single valuation rule is less confusing than per-branch costs on
    /// an item with one sale price. It is a correlated subquery so the page costs one round trip;
    /// the valuation is rounded once the rows are materialised.
    /// </summary>
    private async Task<PagedResult<StockLevelDto>> ProjectStockAsync(
        IQueryable<Product> products,
        long branchId,
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
                Quantity = p.StockLevels.Where(s => s.BranchId == branchId).Sum(s => (int?)s.Quantity) ?? 0,
                // FirstOrDefault rather than Max: the pair is unique so it is the same value, and
                // SQLite refuses to aggregate DateTimeOffset, which would break the test suite.
                UpdatedAt = p.StockLevels.Where(s => s.BranchId == branchId)
                    .Select(s => (DateTimeOffset?)s.UpdatedAt).FirstOrDefault() ?? p.CreatedAt,
                LastCost = db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.MovementType == MovementType.Purchase && m.UnitCost != null)
                    // Ledger ids are monotonic: the highest id is the most recent purchase cost.
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.UnitCost)
                    .FirstOrDefault()
            })
            .ToPagedResultAsync(request, cancellationToken);

        // One lookup for the whole page: the code is constant across it.
        var branchCode = await db.Branches.AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => b.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var items = page.Items
            .Select(r => new StockLevelDto(
                r.Id,
                r.Name,
                r.PartNumber,
                r.TrademarkName,
                r.CategoryName,
                branchId,
                branchCode,
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
        long? branchIdFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (!await db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        var branchId = currentUser.ResolveReadableBranch(branchIdFilter);

        return await db.InventoryMovements.AsNoTracking()
            .Where(m => m.ProductId == productId && m.BranchId == branchId)
            .OrderByDescending(m => m.Id)
            .Select(m => new InventoryMovementDto(
                m.Id,
                m.ProductId,
                m.Product.Name,
                m.BranchId,
                m.Branch.Code,
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

        var branchId = currentUser.RequireWritableBranch(request.BranchId);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await stockLock.LockAsync([new StockKey(branchId, request.ProductId)], ct);

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
                ?? throw new NotFoundException(nameof(Product), request.ProductId);

            var now = clock.GetUtcNow();
            var stock = await GetOrCreateStockLevelAsync(branchId, product.Id, now, ct);
            stock.Apply(request.QuantityDelta, now);

            db.InventoryMovements.Add(new InventoryMovement
            {
                BranchId = branchId,
                ProductId = product.Id,
                MovementType = MovementType.Adjustment,
                QuantityDelta = request.QuantityDelta,
                ReferenceType = "manual",
                Notes = request.Notes?.Trim()
            });

            await db.SaveChangesAsync(ct);

            // Re-read through the shared projection so the caller gets the same shape as the list,
            // revalued at the product's last purchase cost.
            var page = await ProjectStockAsync(
                db.Products.AsNoTracking().Where(p => p.Id == product.Id),
                branchId,
                new PageRequest(null, 1, 1),
                ct);

            return page.Items[0];
        }, cancellationToken);
    }

    public async Task<PagedResult<KardexRowDto>> GetKardexAsync(KardexQuery query, CancellationToken cancellationToken = default)
    {
        var branchId = currentUser.ResolveReadableBranch(query.BranchId);
        var request = query.ToPageRequest();

        var products = db.Products.AsNoTracking().AsQueryable();
        if (request.TrimmedSearch is { } search)
        {
            products = products.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{search.ToLower()}%"));
        }

        return await products
            .OrderBy(p => p.Name)
            .Select(p => new KardexRowDto(
                p.Id,
                p.Name,
                p.PartNumber,
                p.Trademark != null ? p.Trademark.Name : null,
                branchId,
                db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.BranchId == branchId && m.MovementType == MovementType.Purchase)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0,
                -(db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.BranchId == branchId && m.MovementType == MovementType.Sale)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0),
                db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.BranchId == branchId && m.MovementType == MovementType.Adjustment)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0,
                db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.BranchId == branchId && m.MovementType == MovementType.TransferIn)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0,
                -(db.InventoryMovements
                    .Where(m => m.ProductId == p.Id && m.BranchId == branchId && m.MovementType == MovementType.TransferOut)
                    .Sum(m => (int?)m.QuantityDelta) ?? 0),
                // Through Sales so the soft-delete filter applies; SaleItem carries none of its own.
                // The branch filter goes on the sale, not the line.
                db.Sales
                    .Where(s => s.BranchId == branchId)
                    .SelectMany(s => s.Items)
                    .Where(i => i.ProductId == p.Id)
                    .Sum(i => (decimal?)i.Subtotal) ?? 0m,
                p.StockLevels.Where(s => s.BranchId == branchId).Sum(s => (int?)s.Quantity) ?? 0))
            .ToPagedResultAsync(request, cancellationToken);
    }

    /// <summary>
    /// Safety net for a (branch, product) pair that somehow has no row. With the grid kept dense by
    /// product creation, branch creation and the backfill this is effectively dead code — and it has
    /// to stay that way, because a row created here was never locked, so it cannot protect against
    /// oversell. The <c>ck_stock_levels_quantity_non_negative</c> check is the real last line.
    /// </summary>
    internal async Task<StockLevel> GetOrCreateStockLevelAsync(
        long branchId,
        long productId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var stock = await db.StockLevels
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.ProductId == productId, cancellationToken);

        if (stock is not null)
        {
            return stock;
        }

        stock = new StockLevel { BranchId = branchId, ProductId = productId, Quantity = 0, UpdatedAt = now };
        db.StockLevels.Add(stock);
        return stock;
    }
}
