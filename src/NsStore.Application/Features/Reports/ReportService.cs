using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Clients;
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
    ClientService clients,
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

        // Totals come from the whole filtered set, not from page.Items: the rows are capped at
        // ReportPageSize, and a sheet that says "347 sales" over the sum of 200 of them is worse
        // than one that says nothing.
        var totals = await SaleTotalsAsync(
            new SaleQuery(search, range.From, range.To, null, 1, ReportPageSize, branchId),
            debtsOnly: false,
            cancellationToken);

        return new SalesReportDto(
            range.From,
            range.To,
            page.Total,
            totals.Quantity,
            totals.Amount,
            totals.Paid,
            totals.Balance,
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

        // Same reason as the sales report: the rows are capped, the footer must not be.
        var scoped = currentUser.ResolveScopedBranch(branchId);
        var all = db.Purchases.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{SearchText.Normalize(search.Trim())}%";
            all = all.Where(p => EF.Functions.Like(SearchText.Unaccent(p.Supplier.Name).ToLower(), pattern));
        }

        if (range.From is { } purchasesFrom)
        {
            all = all.Where(p => p.PurchaseDate >= purchasesFrom);
        }

        if (range.To is { } purchasesTo)
        {
            all = all.Where(p => p.PurchaseDate <= purchasesTo);
        }

        if (scoped is { } scopedBranch)
        {
            all = all.Where(p => p.BranchId == scopedBranch);
        }

        var totals = await all
            .GroupBy(_ => 1)
            .Select(g => new { Quantity = g.Sum(p => p.TotalQuantity), Amount = g.Sum(p => p.TotalAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        return new PurchasesReportDto(
            range.From,
            range.To,
            page.Total,
            totals?.Quantity ?? 0,
            totals?.Amount ?? 0m,
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

        var totals = await SaleTotalsAsync(
            new SaleQuery(search, null, null, null, 1, ReportPageSize, branchId),
            debtsOnly: true,
            cancellationToken);

        return new DebtsReportDto(page.Total, totals.Balance, page.Items);
    }

    /// <summary>
    /// Sums the filtered set server-side, independent of the row cap the sheet prints.
    /// </summary>
    /// <remarks>
    /// Reimplements the filter rather than reusing <c>SaleService.Filter</c>, which is private and
    /// paging-shaped. Keep the two in step: a divergence shows up as a footer that disagrees with
    /// the rows above it.
    /// </remarks>
    private async Task<(int Quantity, decimal Amount, decimal Paid, decimal Balance)> SaleTotalsAsync(
        SaleQuery query,
        bool debtsOnly,
        CancellationToken cancellationToken)
    {
        var scoped = currentUser.ResolveScopedBranch(query.BranchId);
        var sales = db.Sales.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{SearchText.Normalize(query.Search.Trim())}%";
            sales = sales.Where(s =>
                EF.Functions.Like(SearchText.Unaccent(s.Client.Name).ToLower(), pattern) ||
                (s.Client.LastName != null && EF.Functions.Like(SearchText.Unaccent(s.Client.LastName).ToLower(), pattern)) ||
                (s.Client.MotherLastName != null && EF.Functions.Like(SearchText.Unaccent(s.Client.MotherLastName).ToLower(), pattern)));
        }

        if (query.From is { } from)
        {
            sales = sales.Where(s => s.SaleDate >= from);
        }

        if (query.To is { } to)
        {
            sales = sales.Where(s => s.SaleDate <= to);
        }

        if (scoped is { } branchId)
        {
            sales = sales.Where(s => s.BranchId == branchId);
        }

        if (debtsOnly)
        {
            sales = sales.Where(s => s.PaymentStatus == PaymentStatus.Credit && s.TotalPaid < s.TotalAmount);
        }

        var totals = await sales
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Quantity = g.Sum(s => s.TotalQuantity),
                Amount = g.Sum(s => s.TotalAmount),
                Paid = g.Sum(s => s.TotalPaid)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return totals is null
            ? (0, 0m, 0m, 0m)
            : (totals.Quantity, totals.Amount, totals.Paid, totals.Amount - totals.Paid);
    }

    /// <summary>
    /// Everything one client still owes, with the instalments already credited to each sale.
    /// </summary>
    /// <remarks>
    /// Filters on the balance rather than <c>PaymentStatus == Credit</c> as the debts list does: if
    /// a sale ever ended up flagged paid while still carrying a balance, the customer's own
    /// statement is the last place that should quietly hide it.
    /// </remarks>
    public async Task<ClientStatementDto> GetClientStatementAsync(long clientId, CancellationToken cancellationToken = default)
    {
        var client = await clients.GetAsync(clientId, cancellationToken);
        var overdueDays = (await settings.GetAsync(cancellationToken)).OverdueDays;
        var today = clock.Today();
        var branchId = currentUser.ResolveScopedBranch();

        var rows = await db.Sales.AsNoTracking()
            .Where(s => s.ClientId == clientId
                && s.TotalPaid < s.TotalAmount
                && (branchId == null || s.BranchId == branchId))
            .OrderBy(s => s.SaleDate)
            .ThenBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.Number,
                BranchCode = s.Branch.Code,
                s.SaleDate,
                s.InvoiceType,
                s.TotalAmount,
                s.TotalPaid,
                Payments = s.Payments
                    .OrderBy(p => p.PaymentDate)
                    .ThenBy(p => p.Id)
                    .Select(p => new PaymentDto(
                        p.Id,
                        p.SaleId,
                        p.BranchId,
                        p.Amount,
                        p.PaymentDate,
                        p.CreatedAt,
                        db.Users.Where(u => u.Id == p.CreatedBy).Select(u => u.Username).FirstOrDefault()))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var lastPayment = rows
            .SelectMany(r => r.Payments)
            .Select(p => (DateOnly?)p.PaymentDate)
            .DefaultIfEmpty(null)
            .Max();

        var sales = rows.Select(r =>
        {
            var since = r.Payments.Count > 0 ? r.Payments.Max(p => p.PaymentDate) : r.SaleDate;
            var days = Math.Max(0, today.DayNumber - since.DayNumber);

            return new ClientStatementSaleDto(
                r.Id,
                r.Number,
                r.BranchCode,
                r.SaleDate,
                r.InvoiceType,
                r.TotalAmount,
                r.TotalPaid,
                r.TotalAmount - r.TotalPaid,
                days,
                days > overdueDays,
                r.Payments);
        }).ToList();

        return new ClientStatementDto(
            client,
            today,
            overdueDays,
            sales.Count,
            sales.Sum(s => s.TotalAmount),
            sales.Sum(s => s.TotalPaid),
            sales.Sum(s => s.Balance),
            sales.Count > 0 ? sales.Min(s => s.SaleDate) : null,
            lastPayment,
            sales);
    }

    /// <summary>Prices are global; only the quantity column is branch-specific.</summary>
    public async Task<PriceListReportDto> GetPriceListAsync(string? search, long? branchId = null, CancellationToken cancellationToken = default)
    {
        var scope = currentUser.ResolveReadableBranch(branchId);
        var query = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{SearchText.Normalize(search.Trim())}%";
            query = query.Where(p => EF.Functions.Like(SearchText.Unaccent(p.Name).ToLower(), pattern));
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
        var today = clock.Today();
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
