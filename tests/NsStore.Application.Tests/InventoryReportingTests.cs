using NsStore.Application.Common.Models;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// Covers the figures the stock and kardex screens print: last purchase cost, inventory
/// valuation, adjustments and the money actually invoiced per product.
/// </summary>
public class InventoryReportingTests
{
    private static Task<PurchaseDto> BuyAsync(TestHarness harness, long productId, int quantity, decimal unitPrice) =>
        harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, quantity, unitPrice)]));

    [Fact]
    public async Task Stock_rows_carry_the_last_purchase_cost_and_its_valuation()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        await BuyAsync(harness, productId, quantity: 10, unitPrice: 100m);
        // A later purchase at a different price is the one valuation must use.
        await BuyAsync(harness, productId, quantity: 5, unitPrice: 120m);

        var row = Assert.Single((await harness.Inventory.ListStockAsync(new StockQuery(null))).Items);

        Assert.Equal(15, row.Quantity);
        Assert.Equal(120m, row.LastCost);
        Assert.Equal(1800m, row.InventoryValue); // 15 × 120
    }

    [Fact]
    public async Task Stock_rows_without_purchase_history_have_no_cost_and_no_value()
    {
        using var harness = new TestHarness();
        await harness.CreateProductAsync();

        var row = Assert.Single((await harness.Inventory.ListStockAsync(new StockQuery(null))).Items);

        Assert.Null(row.LastCost);
        Assert.Equal(0m, row.InventoryValue);
    }

    [Fact]
    public async Task Adjusting_stock_returns_the_revalued_row()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();
        await BuyAsync(harness, productId, quantity: 10, unitPrice: 50m);

        var adjusted = await harness.Inventory.AdjustAsync(
            new StockAdjustmentRequest(productId, QuantityDelta: -3, Notes: "Conteo físico"));

        Assert.Equal(7, adjusted.Quantity);
        Assert.Equal(50m, adjusted.LastCost);
        Assert.Equal(350m, adjusted.InventoryValue);
    }

    [Fact]
    public async Task Kardex_reports_adjustments_so_the_balance_reconciles()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        await BuyAsync(harness, productId, quantity: 20, unitPrice: 100m);
        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(150.80m, 130m));
        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, Quantity: 5)]));
        await harness.Inventory.AdjustAsync(new StockAdjustmentRequest(productId, QuantityDelta: -2, Notes: "Merma"));

        var row = Assert.Single((await harness.Inventory.GetKardexAsync(new KardexQuery(null))).Items);

        Assert.Equal(20, row.TotalPurchased);
        Assert.Equal(5, row.TotalSold);
        Assert.Equal(-2, row.TotalAdjusted);
        Assert.Equal(13, row.Available);
        Assert.Equal(row.TotalPurchased - row.TotalSold + row.TotalAdjusted, row.Available);
    }

    [Fact]
    public async Task Kardex_sums_what_was_actually_invoiced_not_units_at_todays_price()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        await BuyAsync(harness, productId, quantity: 20, unitPrice: 100m);
        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(150.80m, 130m));
        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, Quantity: 4)]));

        // Repricing afterwards must not rewrite the history of what was already sold.
        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(232m, 200m));

        var row = Assert.Single((await harness.Inventory.GetKardexAsync(new KardexQuery(null))).Items);

        Assert.Equal(520m, row.TotalSoldAmount); // 4 × 130, the price charged at sale time
    }

    [Fact]
    public async Task Kardex_rows_carry_the_part_number_and_trademark_the_report_prints()
    {
        using var harness = new TestHarness();

        harness.Db.Trademarks.Add(new Domain.Entities.Trademark { Id = 1, Name = "Kingston" });
        await harness.Db.SaveChangesAsync();

        var product = await harness.Products.CreateAsync(
            new ProductRequest("SSD 1TB", "SA400S37", null, IsSerialized: false, TrademarkId: 1, null, null));

        var row = Assert.Single((await harness.Inventory.GetKardexAsync(new KardexQuery(null))).Items);

        Assert.Equal(product.Id, row.ProductId);
        Assert.Equal("SA400S37", row.PartNumber);
        Assert.Equal("Kingston", row.TrademarkName);
    }

    [Fact]
    public async Task Purchase_rows_report_how_many_lines_they_hold()
    {
        using var harness = new TestHarness();
        var first = await harness.CreateProductAsync("SSD 1TB");
        var second = await harness.CreateProductAsync("RAM 8GB");

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [
                new PurchaseItemRequest(first, Quantity: 10, UnitPrice: 100m),
                new PurchaseItemRequest(second, Quantity: 4, UnitPrice: 55m)
            ]));

        var row = Assert.Single(
            (await harness.Purchases.ListAsync(new PurchaseQuery(null, null, null))).Items);

        Assert.Equal(2, row.LineCount);
        Assert.Equal(14, row.TotalQuantity);
    }
}
