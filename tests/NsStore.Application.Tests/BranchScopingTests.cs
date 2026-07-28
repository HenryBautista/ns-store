using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// The asymmetry this feature is built on: writes are pinned to the active branch, and one branch's
/// stock is untouchable from another.
/// </summary>
public class BranchScopingTests
{
    private static async Task<long> ReadyProductAsync(TestHarness harness, int quantity = 10)
    {
        var productId = await harness.CreateProductAsync();

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, quantity, UnitPrice: 100m)]));

        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(150.80m, 130m));
        return productId;
    }

    private static Task<SaleDto> SellOneAsync(TestHarness harness, long productId) =>
        harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, Quantity: 1)]));

    [Fact]
    public async Task A_sellers_sale_is_stamped_with_their_home_branch()
    {
        using var harness = new TestHarness(userId: 1, UserRole.Seller, branchId: TestHarness.MainBranchId);
        var productId = await ReadyProductAsync(harness);

        var sale = await SellOneAsync(harness, productId);

        Assert.Equal(TestHarness.MainBranchId, sale.BranchId);
        Assert.Equal("MAIN", sale.BranchCode);
    }

    [Fact]
    public async Task A_seller_cannot_write_into_another_branch()
    {
        using var harness = new TestHarness(userId: 1, UserRole.Seller, branchId: TestHarness.MainBranchId);
        var productId = await ReadyProductAsync(harness);

        // A stale or tampered X-Branch-Id lands here: a header pointing somewhere else than the
        // seller's home branch is rejected, never silently ignored.
        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;

        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => SellOneAsync(harness, productId));

        Assert.Equal(ErrorCodes.BranchNotAllowed, exception.ErrorCode);
    }

    [Fact]
    public async Task An_admin_switching_branches_writes_into_the_branch_they_switched_to()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, 4, UnitPrice: 100m)]));

        var south = await harness.Db.StockLevels
            .SingleAsync(s => s.BranchId == TestHarness.SouthBranchId && s.ProductId == productId);
        var main = await harness.Db.StockLevels
            .SingleAsync(s => s.BranchId == TestHarness.MainBranchId && s.ProductId == productId);

        Assert.Equal(4, south.Quantity);
        Assert.Equal(10, main.Quantity);
    }

    [Fact]
    public async Task A_sale_in_one_branch_leaves_the_other_branchs_stock_untouched()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        await SellOneAsync(harness, productId);

        var south = await harness.Db.StockLevels
            .SingleAsync(s => s.BranchId == TestHarness.SouthBranchId && s.ProductId == productId);

        Assert.Equal(0, south.Quantity);
    }

    [Fact]
    public async Task Stock_available_in_another_branch_does_not_make_a_sale_possible()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 5);

        // Five units exist — all of them in MAIN. Selling from SUR must still fail.
        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => SellOneAsync(harness, productId));

        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);
    }

    [Fact]
    public async Task Product_listings_report_the_quantity_of_the_active_branch()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 7);

        var inMain = await harness.Products.GetAsync(productId);
        Assert.Equal(7, inMain.AvailableQuantity);

        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;
        var inSouth = await harness.Products.GetAsync(productId);
        Assert.Equal(0, inSouth.AvailableQuantity);
    }

    [Fact]
    public async Task A_credit_can_be_collected_at_a_different_branch_than_the_sale()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Credit,
            InitialPaid: 0m,
            // Two units (260) settled with 100: SQLite stores decimals as TEXT, so
            // ck_sales_total_paid_within_total compares them lexicographically. Amounts of equal
            // digit length keep the check honest here; against Postgres numeric it is a non-issue.
            [new SaleItemRequest(productId, Quantity: 2)]));

        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;
        await harness.Sales.RegisterPaymentAsync(sale.Id, new RegisterPaymentRequest(100m, null));

        var payment = await harness.Db.Payments.SingleAsync(p => p.SaleId == sale.Id);

        // Payment.BranchId is the till that took the money, not the branch that sold.
        Assert.Equal(TestHarness.SouthBranchId, payment.BranchId);
        Assert.Equal(TestHarness.MainBranchId, sale.BranchId);
    }
}
