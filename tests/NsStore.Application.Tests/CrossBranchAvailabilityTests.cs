using NsStore.Application.Common;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// The reading half of the asymmetry: stock is visible everywhere, money is not.
/// </summary>
public class CrossBranchAvailabilityTests
{
    private static async Task<long> StockedInMainAsync(TestHarness harness, int quantity = 3)
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

    [Fact]
    public async Task Availability_reports_every_active_branch_including_the_empty_ones()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        var rows = await harness.Inventory.GetAvailabilityAsync(productId);

        Assert.Equal(2, rows.Count);
        Assert.Equal(3, rows.Single(r => r.BranchId == TestHarness.MainBranchId).Quantity);
        Assert.Equal(0, rows.Single(r => r.BranchId == TestHarness.SouthBranchId).Quantity);
        Assert.Equal("MAIN", rows.Single(r => r.BranchId == TestHarness.MainBranchId).BranchCode);
    }

    /// <summary>Moves the caller to a seller whose home branch is SUR, without a new database.</summary>
    private static void BecomeSouthSeller(TestHarness harness)
    {
        harness.CurrentUser.Role = UserRole.Seller;
        harness.CurrentUser.HomeBranchId = TestHarness.SouthBranchId;
        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;
    }

    [Fact]
    public async Task A_seller_standing_in_an_empty_branch_can_still_see_where_the_stock_is()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        // The motivating case: a seller in SUR with none on the shelf tells the customer where to go.
        BecomeSouthSeller(harness);

        var rows = await harness.Inventory.GetAvailabilityAsync(productId);

        Assert.Contains(rows, r => r.BranchId == TestHarness.MainBranchId && r.Quantity == 3);
    }

    [Fact]
    public async Task Product_search_carries_both_the_local_and_the_system_wide_quantity()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;
        var product = await harness.Products.GetAsync(productId);

        // This single field is what paints the POS "in stock elsewhere" chip with no extra request.
        Assert.Equal(0, product.AvailableQuantity);
        Assert.Equal(3, product.QuantityAllBranches);
    }

    [Fact]
    public async Task Stock_can_be_read_for_another_branch_without_switching_to_it()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);
        BecomeSouthSeller(harness);

        // A seller asking about MAIN is a plain read, never a 403.
        var page = await harness.Inventory.ListStockAsync(new StockQuery(null, TestHarness.MainBranchId));

        var row = page.Items.Single(i => i.ProductId == productId);
        Assert.Equal(3, row.Quantity);
        Assert.Equal(TestHarness.MainBranchId, row.BranchId);
    }

    [Fact]
    public async Task A_seller_asking_for_another_branchs_sales_gets_their_own_instead()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness, quantity: 10);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        BecomeSouthSeller(harness);

        // Asking for MAIN's sales is silently pinned back to SUR rather than rejected.
        var page = await harness.Sales.ListAsync(new SaleQuery(null, null, null, null, BranchId: TestHarness.MainBranchId));

        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task An_admin_asking_for_no_branch_sees_every_branchs_sales()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness, quantity: 10);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        var page = await harness.Sales.ListAsync(new SaleQuery(null, null, null, null));

        Assert.Single(page.Items);
    }
}
