using NsStore.Application.Common;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Reports;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// Amounts are chosen so their decimals still compare correctly as TEXT: the suite runs on SQLite,
/// where <c>ck_sales_total_paid_within_total</c> compares strings (see <c>DemoDataSeeder</c>).
/// </summary>
public class ReportServiceTests
{
    private static async Task<long> ReadyProductAsync(TestHarness harness, int quantity = 40)
    {
        var productId = await harness.CreateProductAsync();

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, quantity, UnitPrice: 50m)]));

        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(
            PriceWithInvoice: 116m,
            PriceWithoutInvoice: 100m));

        return productId;
    }

    [Fact]
    public async Task The_statement_lists_only_what_the_client_still_owes()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var owed = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Credit, 0m,
            [new SaleItemRequest(productId, 2)]));

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        var statement = await harness.Reports.GetClientStatementAsync(1);

        var sale = Assert.Single(statement.Sales);
        Assert.Equal(owed.Id, sale.SaleId);
        Assert.Equal(200m, statement.TotalDebt);
        Assert.Equal(1, statement.SaleCount);
        Assert.Equal("Juan Perez", statement.Client.FullName);
    }

    [Fact]
    public async Task The_statement_credits_each_sale_with_the_instalments_already_paid()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Credit, 0m,
            [new SaleItemRequest(productId, 3)]));

        await harness.Sales.RegisterPaymentAsync(sale.Id, new RegisterPaymentRequest(100m, harness.Today));

        var statement = await harness.Reports.GetClientStatementAsync(1);
        var row = Assert.Single(statement.Sales);

        var payment = Assert.Single(row.Payments);
        Assert.Equal(100m, payment.Amount);
        Assert.Equal(200m, row.Balance);
        Assert.Equal(harness.Today, statement.LastPaymentDate);
    }

    [Fact]
    public async Task A_statement_for_an_unknown_client_is_a_not_found()
    {
        using var harness = new TestHarness();

        await Assert.ThrowsAsync<NotFoundException>(() => harness.Reports.GetClientStatementAsync(9999));
    }

    [Fact]
    public async Task A_client_with_no_debt_gets_an_empty_statement_rather_than_an_error()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        var statement = await harness.Reports.GetClientStatementAsync(1);

        Assert.Empty(statement.Sales);
        Assert.Equal(0m, statement.TotalDebt);
        Assert.Null(statement.OldestSaleDate);
    }

    /// <summary>
    /// 01:30 UTC is 21:30 the previous evening in Bolivia — the window where the shop is still open
    /// but UTC has already turned the page. The dashboard has to answer for the counter's day, not
    /// the server's, or the last hours of every day's takings vanish from it.
    /// </summary>
    [Fact]
    public async Task An_evening_sale_still_counts_as_todays_on_the_dashboard()
    {
        using var harness = new TestHarness(now: new DateTimeOffset(2026, 7, 24, 1, 30, 0, TimeSpan.Zero));
        var productId = await ReadyProductAsync(harness);

        Assert.Equal(new DateOnly(2026, 7, 23), harness.Today);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        var dashboard = await harness.Reports.GetDashboardAsync();

        Assert.Equal(new DateOnly(2026, 7, 23), dashboard.Date);
        Assert.Equal(1, dashboard.SalesTodayCount);
        Assert.Equal(100m, dashboard.SalesTodayAmount);
    }

    [Fact]
    public async Task Report_totals_cover_the_whole_filtered_set_not_just_the_printed_page()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 400);

        // More sales than a sheet prints, so the footer and the rows must disagree in length but
        // not in money. Anything above ReportPageSize (200) exercises the cap.
        for (var i = 0; i < 205; i++)
        {
            await harness.Sales.CreateAsync(new CreateSaleRequest(
                harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
                [new SaleItemRequest(productId, 1)]));
        }

        var report = await harness.Reports.GetSalesReportAsync(new ReportRange(null, null), null);

        Assert.Equal(205, report.SaleCount);
        Assert.Equal(200, report.Sales.Count);
        Assert.Equal(20_500m, report.TotalAmount);
        Assert.Equal(205, report.TotalQuantity);
    }
}
