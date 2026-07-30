using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Application.Features.Settings;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Reports;

public class ReportService(
    IAppDbContext db,
    SaleService sales,
    PurchaseService purchases,
    InventoryService inventory,
    SettingsService settings,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    private const int ReportPageSize = PageRequest.MaxPageSize;

    public async Task<SalesReportDto> GetSalesReportAsync(
        ReportRange range,
        string? search,
        long? branchId = null,
        CancellationToken cancellationToken = default)
    {
        // Scoping is inherited for free: SaleService.ListAsync already pins a non-admin.
        var page = await sales.ListAsync(
            new SaleQuery(search, range.From, range.To, null, 1, ReportPageSize, branchId),
            cancellationToken);

        return new SalesReportDto(
            range.From,
            range.To,
            page.Total,
            page.Items.Sum(s => s.TotalQuantity),
            page.Items.Sum(s => s.TotalAmount),
            page.Items.Sum(s => s.TotalPaid),
            page.Items.Sum(s => s.Balance),
            page.Items);
    }

    public async Task<PurchasesReportDto> GetPurchasesReportAsync(
        ReportRange range,
        string? search,
        long? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var page = await purchases.ListAsync(
            new PurchaseQuery(search, range.From, range.To, 1, ReportPageSize, branchId),
            cancellationToken);

        return new PurchasesReportDto(
            range.From,
            range.To,
            page.Total,
            page.Items.Sum(p => p.TotalQuantity),
            page.Items.Sum(p => p.TotalAmount),
            page.Items);
    }

    public async Task<StockReportDto> GetStockReportAsync(string? search, long? branchId = null, CancellationToken cancellationToken = default)
    {
        var page = await inventory.ListStockAsync(new StockQuery(search, branchId, 1, ReportPageSize), cancellationToken);

        return new StockReportDto(
            page.Total,
            page.Items.Sum(i => i.Quantity),
            page.Items.Sum(i => i.InventoryValue),
            page.Items);
    }

    public async Task<DebtsReportDto> GetDebtsReportAsync(string? search, long? branchId = null, CancellationToken cancellationToken = default)
    {
        var page = await sales.ListDebtsAsync(
            new SaleQuery(search, null, null, null, 1, ReportPageSize, branchId),
            cancellationToken);

        return new DebtsReportDto(page.Total, page.Items.Sum(s => s.Balance), page.Items);
    }

    /// <summary>Prices are global; only the quantity column is branch-specific.</summary>
    public async Task<PriceListReportDto> GetPriceListAsync(string? search, long? branchId = null, CancellationToken cancellationToken = default)
    {
        var scope = currentUser.ResolveReadableBranch(branchId);
        var query = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{search.Trim().ToLower()}%"));
        }

        var items = await query
            .OrderBy(p => p.Name)
            .Select(p => new PriceListRowDto(
                p.Id,
                p.Name,
                p.PartNumber,
                p.Trademark != null ? p.Trademark.Name : null,
                p.Category != null ? p.Category.Name : null,
                p.PriceWithInvoice,
                p.PriceWithoutInvoice,
                p.StockLevels.Where(s => s.BranchId == scope).Sum(s => (int?)s.Quantity) ?? 0,
                // The price list is a global document, so showing both figures is the useful form.
                p.StockLevels.Sum(s => (int?)s.Quantity) ?? 0))
            .ToListAsync(cancellationToken);

        var appSettings = await settings.GetAsync(cancellationToken);
        return new PriceListReportDto(appSettings.Currency, items);
    }

    public async Task<WarrantyNoteDto> GetWarrantyNoteAsync(long saleId, CancellationToken cancellationToken = default)
    {
        var sale = await sales.GetAsync(saleId, cancellationToken);
        var noteType = sale.PaymentStatus == PaymentStatus.Paid ? "standard" : "credit";
        return new WarrantyNoteDto(noteType, sale);
    }

    /// <summary>
    /// Money and stock figures are scoped; the catalog counts stay global. A null
    /// <paramref name="branchId"/> from an admin means every branch — a seller is always pinned.
    /// </summary>
    public async Task<DashboardDto> GetDashboardAsync(long? branchId = null, CancellationToken cancellationToken = default)
    {
        var scope = currentUser.ResolveScopedBranch(branchId);
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var salesToday = await db.Sales.AsNoTracking()
            .Where(s => (scope == null || s.BranchId == scope) && s.SaleDate == today)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(s => s.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var salesMonth = await db.Sales.AsNoTracking()
            .Where(s => (scope == null || s.BranchId == scope) && s.SaleDate >= monthStart && s.SaleDate <= today)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(s => s.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var purchasesMonth = await db.Purchases.AsNoTracking()
            .Where(p => (scope == null || p.BranchId == scope) && p.PurchaseDate >= monthStart && p.PurchaseDate <= today)
            .SumAsync(p => (decimal?)p.TotalAmount, cancellationToken) ?? 0m;

        var debts = await db.Sales.AsNoTracking()
            .Where(s => (scope == null || s.BranchId == scope)
                && s.PaymentStatus == PaymentStatus.Credit
                && s.TotalPaid < s.TotalAmount)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(s => s.TotalAmount - s.TotalPaid) })
            .FirstOrDefaultAsync(cancellationToken);

        var productCount = await db.Products.AsNoTracking().CountAsync(cancellationToken);
        var stockUnits = await db.StockLevels.AsNoTracking()
            .Where(s => scope == null || s.BranchId == scope)
            .SumAsync(s => (int?)s.Quantity, cancellationToken) ?? 0;

        // !Any(...) preserves the old semantics exactly: a product with no stock row counts as out.
        var outOfStock = await db.Products.AsNoTracking()
            .CountAsync(p => !p.StockLevels.Any(s => (scope == null || s.BranchId == scope) && s.Quantity > 0), cancellationToken);
        var pendingOrders = await db.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var quoteCount = await db.Quotes.AsNoTracking().CountAsync(cancellationToken);

        return new DashboardDto(
            scope,
            today,
            salesToday?.Amount ?? 0m,
            salesToday?.Count ?? 0,
            salesMonth?.Amount ?? 0m,
            salesMonth?.Count ?? 0,
            purchasesMonth,
            debts?.Amount ?? 0m,
            debts?.Count ?? 0,
            productCount,
            stockUnits,
            outOfStock,
            pendingOrders,
            quoteCount);
    }
}
