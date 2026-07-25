using Microsoft.EntityFrameworkCore;
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
    TimeProvider clock)
{
    private const int ReportPageSize = PageRequest.MaxPageSize;

    public async Task<SalesReportDto> GetSalesReportAsync(ReportRange range, string? search, CancellationToken cancellationToken = default)
    {
        var page = await sales.ListAsync(
            new SaleQuery(search, range.From, range.To, null, 1, ReportPageSize),
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

    public async Task<PurchasesReportDto> GetPurchasesReportAsync(ReportRange range, string? search, CancellationToken cancellationToken = default)
    {
        var page = await purchases.ListAsync(
            new PurchaseQuery(search, range.From, range.To, 1, ReportPageSize),
            cancellationToken);

        return new PurchasesReportDto(
            range.From,
            range.To,
            page.Total,
            page.Items.Sum(p => p.TotalQuantity),
            page.Items.Sum(p => p.TotalAmount),
            page.Items);
    }

    public async Task<StockReportDto> GetStockReportAsync(string? search, CancellationToken cancellationToken = default)
    {
        var page = await inventory.ListStockAsync(new PageRequest(search, 1, ReportPageSize), cancellationToken);
        return new StockReportDto(page.Total, page.Items.Sum(i => i.Quantity), page.Items);
    }

    public async Task<DebtsReportDto> GetDebtsReportAsync(string? search, CancellationToken cancellationToken = default)
    {
        var page = await sales.ListDebtsAsync(
            new SaleQuery(search, null, null, null, 1, ReportPageSize),
            cancellationToken);

        return new DebtsReportDto(page.Total, page.Items.Sum(s => s.Balance), page.Items);
    }

    public async Task<PriceListReportDto> GetPriceListAsync(string? search, CancellationToken cancellationToken = default)
    {
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
                p.StockLevel != null ? p.StockLevel.Quantity : 0))
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

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var salesToday = await db.Sales.AsNoTracking()
            .Where(s => s.SaleDate == today)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(s => s.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var salesMonth = await db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= monthStart && s.SaleDate <= today)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(s => s.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var purchasesMonth = await db.Purchases.AsNoTracking()
            .Where(p => p.PurchaseDate >= monthStart && p.PurchaseDate <= today)
            .SumAsync(p => (decimal?)p.TotalAmount, cancellationToken) ?? 0m;

        var debts = await db.Sales.AsNoTracking()
            .Where(s => s.PaymentStatus == PaymentStatus.Credit && s.TotalPaid < s.TotalAmount)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(s => s.TotalAmount - s.TotalPaid) })
            .FirstOrDefaultAsync(cancellationToken);

        var productCount = await db.Products.AsNoTracking().CountAsync(cancellationToken);
        var stockUnits = await db.StockLevels.AsNoTracking().SumAsync(s => (int?)s.Quantity, cancellationToken) ?? 0;
        var outOfStock = await db.Products.AsNoTracking()
            .CountAsync(p => p.StockLevel == null || p.StockLevel.Quantity == 0, cancellationToken);
        var pendingOrders = await db.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var quoteCount = await db.Quotes.AsNoTracking().CountAsync(cancellationToken);

        return new DashboardDto(
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
