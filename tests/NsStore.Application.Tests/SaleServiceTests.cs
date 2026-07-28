using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

public class SaleServiceTests
{
    /// <summary>Product with stock and both sale prices set — the normal POS starting point.</summary>
    private static async Task<long> ReadyProductAsync(TestHarness harness, int quantity = 10)
    {
        var productId = await harness.CreateProductAsync();

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, quantity, UnitPrice: 100m)]));

        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(
            PriceWithInvoice: 150.80m,
            PriceWithoutInvoice: 130m));

        return productId;
    }

    [Fact]
    public async Task Cash_sale_decrements_stock_prices_by_invoice_type_and_settles_immediately()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, Quantity: 2)]));

        Assert.Equal(2, sale.TotalQuantity);
        Assert.Equal(260m, sale.TotalAmount);          // 2 × 130 (price without invoice)
        Assert.Equal(260m, sale.TotalPaid);
        Assert.Equal(0m, sale.Balance);
        Assert.Equal(PaymentStatus.Paid, sale.PaymentStatus);
        Assert.Single(sale.Payments);

        var stock = await harness.Db.StockLevels
            .SingleAsync(s => s.ProductId == productId && s.BranchId == TestHarness.MainBranchId);
        Assert.Equal(8, stock.Quantity);

        var movement = await harness.Db.InventoryMovements
            .SingleAsync(m => m.ProductId == productId && m.MovementType == MovementType.Sale);
        Assert.Equal(-2, movement.QuantityDelta);
        Assert.Equal(sale.Id, movement.ReferenceId);
    }

    [Fact]
    public async Task Sale_with_invoice_uses_the_invoiced_price()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, Quantity: 1)]));

        Assert.Equal(150.80m, sale.TotalAmount);
    }

    [Fact]
    public async Task Credit_sale_keeps_the_balance_and_tracks_installments()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Credit,
            InitialPaid: 100m,
            [new SaleItemRequest(productId, Quantity: 2)]));

        Assert.Equal(160m, sale.Balance);
        Assert.Equal(PaymentStatus.Credit, sale.PaymentStatus);

        var afterPayment = await harness.Sales.RegisterPaymentAsync(sale.Id, new RegisterPaymentRequest(60m, harness.Today));
        Assert.Equal(100m, afterPayment.Balance);
        Assert.Equal(2, afterPayment.Payments.Count);

        var settled = await harness.Sales.RegisterPaymentAsync(sale.Id, new RegisterPaymentRequest(100m, harness.Today));
        Assert.Equal(0m, settled.Balance);
        Assert.Equal(PaymentStatus.Paid, settled.PaymentStatus);
    }

    [Fact]
    public async Task Payment_above_the_balance_is_rejected()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);
        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Credit,
            InitialPaid: 0m,
            [new SaleItemRequest(productId, Quantity: 1)]));

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() =>
            harness.Sales.RegisterPaymentAsync(sale.Id, new RegisterPaymentRequest(1_000m, harness.Today)));

        Assert.Equal(ErrorCodes.PaymentExceedsBalance, exception.ErrorCode);
    }

    [Fact]
    public async Task Selling_more_than_available_is_rejected_and_leaves_stock_untouched()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 3);

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() =>
            harness.Sales.CreateAsync(new CreateSaleRequest(
                harness.Today,
                ClientId: 1,
                InvoiceType.WithoutInvoice,
                PaymentStatus.Paid,
                InitialPaid: null,
                [new SaleItemRequest(productId, Quantity: 4)])));

        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);

        harness.Db.ChangeTracker.Clear();
        var stock = await harness.Db.StockLevels
            .SingleAsync(s => s.ProductId == productId && s.BranchId == TestHarness.MainBranchId);
        Assert.Equal(3, stock.Quantity);
        Assert.Empty(await harness.Db.Sales.ToListAsync());
    }

    [Fact]
    public async Task Repeated_lines_for_one_product_are_validated_against_the_combined_quantity()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 3);

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() =>
            harness.Sales.CreateAsync(new CreateSaleRequest(
                harness.Today,
                ClientId: 1,
                InvoiceType.WithoutInvoice,
                PaymentStatus.Paid,
                InitialPaid: null,
                [new SaleItemRequest(productId, 2), new SaleItemRequest(productId, 2)])));

        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);
    }

    [Fact]
    public async Task Selling_a_product_without_a_price_is_rejected()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();
        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, 5, 100m)]));

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Sales.CreateAsync(new CreateSaleRequest(
                harness.Today,
                ClientId: 1,
                InvoiceType.WithoutInvoice,
                PaymentStatus.Paid,
                InitialPaid: null,
                [new SaleItemRequest(productId, 1)])));

        Assert.Equal(ErrorCodes.PriceNotSet, exception.ErrorCode);
    }

    [Fact]
    public async Task Debts_list_only_returns_credit_sales_with_an_outstanding_balance()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 20);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        var credit = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Credit, 0m,
            [new SaleItemRequest(productId, 1)]));

        var debts = await harness.Sales.ListDebtsAsync(new SaleQuery(null, null, null, null));

        Assert.Single(debts.Items);
        Assert.Equal(credit.Id, debts.Items[0].Id);
    }

    [Fact]
    public async Task Sale_appears_in_the_kardex_totals()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 10);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 4)]));

        var kardex = await harness.Inventory.GetKardexAsync(new Common.Models.PageRequest());
        var row = kardex.Items.Single(r => r.ProductId == productId);

        Assert.Equal(10, row.TotalPurchased);
        Assert.Equal(4, row.TotalSold);
        Assert.Equal(6, row.Available);
    }
}
