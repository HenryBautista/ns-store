using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// Collecting one amount across several debts. Amounts here are chosen so their decimal values
/// still compare correctly as TEXT: the suite runs on SQLite, where
/// <c>ck_sales_total_paid_within_total</c> compares strings (see <c>DemoDataSeeder</c>).
/// </summary>
public class CollectionTests
{
    private const decimal UnitPrice = 100m;

    /// <summary>A product priced at a round 100 so every allocation below is easy to read.</summary>
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
            PriceWithoutInvoice: UnitPrice));

        return productId;
    }

    /// <summary>Two unpaid credit sales, the older one first: 100 then 200, 300 outstanding.</summary>
    private static async Task<(long Older, long Newer)> TwoDebtsAsync(TestHarness harness)
    {
        var productId = await ReadyProductAsync(harness);

        var older = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today.AddDays(-30), 1, InvoiceType.WithoutInvoice, PaymentStatus.Credit, 0m,
            [new SaleItemRequest(productId, 1)]));

        var newer = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today.AddDays(-5), 1, InvoiceType.WithoutInvoice, PaymentStatus.Credit, 0m,
            [new SaleItemRequest(productId, 2)]));

        return (older.Id, newer.Id);
    }

    [Fact]
    public async Task An_amount_matching_the_oldest_debt_settles_it_and_leaves_the_newer_untouched()
    {
        using var harness = new TestHarness();
        var (older, newer) = await TwoDebtsAsync(harness);

        var receipt = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 100m, null));

        Assert.Equal(100m, receipt.TotalCollected);
        Assert.Equal(200m, receipt.RemainingDebt);

        var allocation = Assert.Single(receipt.Allocations);
        Assert.Equal(older, allocation.SaleId);
        Assert.True(allocation.Settled);

        Assert.Equal(PaymentStatus.Paid, (await harness.Sales.GetAsync(older)).PaymentStatus);
        Assert.Equal(200m, (await harness.Sales.GetAsync(newer)).Balance);
    }

    [Fact]
    public async Task A_partial_amount_spills_from_the_oldest_debt_into_the_next()
    {
        using var harness = new TestHarness();
        var (older, newer) = await TwoDebtsAsync(harness);

        // 200 of the 300 owed: settles the 100 and puts 100 against the 200. The leftover is 100
        // rather than something like 50 because '50' > '200' as TEXT would trip the SQLite check.
        var receipt = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 200m, null));

        Assert.Equal(2, receipt.Allocations.Count);

        var first = receipt.Allocations.Single(a => a.SaleId == older);
        Assert.Equal(100m, first.Applied);
        Assert.True(first.Settled);

        var second = receipt.Allocations.Single(a => a.SaleId == newer);
        Assert.Equal(100m, second.Applied);
        Assert.False(second.Settled);
        Assert.Equal(100m, second.RemainingBalance);

        Assert.Equal(100m, receipt.RemainingDebt);
    }

    [Fact]
    public async Task Collecting_the_whole_balance_clears_the_client_off_the_collections_list()
    {
        using var harness = new TestHarness();
        await TwoDebtsAsync(harness);

        var receipt = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 300m, null));

        Assert.Equal(0m, receipt.RemainingDebt);
        Assert.All(receipt.Allocations, a => Assert.True(a.Settled));

        var debts = await harness.Sales.ListDebtsByClientAsync(new ClientDebtQuery(null));
        Assert.Empty(debts.Items);
    }

    [Fact]
    public async Task Collecting_more_than_the_client_owes_is_rejected_and_writes_nothing()
    {
        using var harness = new TestHarness();
        var (older, _) = await TwoDebtsAsync(harness);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 300.01m, null)));

        Assert.Equal(ErrorCodes.PaymentExceedsBalance, error.ErrorCode);

        // The rollback is the point: a receipt the ledger disagrees with is worse than a refusal.
        Assert.Empty(await harness.Db.PaymentReceipts.ToListAsync());
        Assert.Equal(100m, (await harness.Sales.GetAsync(older)).Balance);

        var debts = await harness.Sales.ListDebtsByClientAsync(new ClientDebtQuery(null));
        Assert.Equal(300m, debts.Items[0].Balance);
    }

    [Fact]
    public async Task Collecting_from_a_client_with_nothing_outstanding_is_rejected()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [new SaleItemRequest(productId, 1)]));

        await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 50m, null)));
    }

    [Fact]
    public async Task The_receipt_is_numbered_per_branch_and_can_be_reissued()
    {
        using var harness = new TestHarness();
        await TwoDebtsAsync(harness);

        var first = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 100m, null));
        var second = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 100m, null));

        Assert.Equal("MAIN-000001", first.Number);
        Assert.Equal("MAIN-000002", second.Number);

        var reissued = await harness.Sales.GetCollectionReceiptAsync(first.ReceiptId);

        Assert.Equal(first.Number, reissued.Number);
        Assert.Equal(first.TotalCollected, reissued.TotalCollected);
        Assert.Equal(first.Allocations.Count, reissued.Allocations.Count);

        // Reissued after the second collection, so it reports where the client stands now.
        Assert.Equal(100m, reissued.RemainingDebt);
    }

    [Fact]
    public async Task Named_allocations_settle_the_chosen_sale_and_leave_an_older_one_untouched()
    {
        using var harness = new TestHarness();
        var (older, newer) = await TwoDebtsAsync(harness);

        // The whole point of allocations: oldest-first would have put this against `older`.
        var receipt = await harness.Sales.CollectFromClientAsync(
            new CollectDebtRequest(1, 200m, null, [new CollectAllocationRequest(newer, 200m)]));

        var allocation = Assert.Single(receipt.Allocations);
        Assert.Equal(newer, allocation.SaleId);
        Assert.True(allocation.Settled);

        Assert.Equal(PaymentStatus.Paid, (await harness.Sales.GetAsync(newer)).PaymentStatus);
        Assert.Equal(100m, (await harness.Sales.GetAsync(older)).Balance);
        Assert.Equal(100m, receipt.RemainingDebt);
    }

    [Fact]
    public async Task A_named_allocation_may_be_a_partial_instalment()
    {
        using var harness = new TestHarness();
        var (older, newer) = await TwoDebtsAsync(harness);

        var receipt = await harness.Sales.CollectFromClientAsync(
            new CollectDebtRequest(1, 100m, null, [new CollectAllocationRequest(newer, 100m)]));

        var allocation = Assert.Single(receipt.Allocations);
        Assert.Equal(newer, allocation.SaleId);
        Assert.False(allocation.Settled);
        Assert.Equal(100m, allocation.RemainingBalance);

        Assert.Equal(PaymentStatus.Credit, (await harness.Sales.GetAsync(newer)).PaymentStatus);
        Assert.Equal(100m, (await harness.Sales.GetAsync(older)).Balance);
    }

    [Fact]
    public async Task Allocations_are_spread_across_every_sale_they_name()
    {
        using var harness = new TestHarness();
        var (older, newer) = await TwoDebtsAsync(harness);

        var receipt = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(
            1, 200m, null,
            [new CollectAllocationRequest(newer, 100m), new CollectAllocationRequest(older, 100m)]));

        Assert.Equal(2, receipt.Allocations.Count);
        Assert.True(receipt.Allocations.Single(a => a.SaleId == older).Settled);
        Assert.Equal(100m, receipt.Allocations.Single(a => a.SaleId == newer).RemainingBalance);
        Assert.Equal(100m, receipt.RemainingDebt);
    }

    [Fact]
    public async Task Allocations_that_do_not_add_up_to_the_collected_amount_are_rejected()
    {
        using var harness = new TestHarness();
        var (_, newer) = await TwoDebtsAsync(harness);

        // The receipt is printed from Amount, so a breakdown that says something else cannot stand.
        await Assert.ThrowsAsync<BadRequestException>(() => harness.Sales.CollectFromClientAsync(
            new CollectDebtRequest(1, 200m, null, [new CollectAllocationRequest(newer, 100m)])));

        Assert.Empty(await harness.Db.PaymentReceipts.ToListAsync());
    }

    [Fact]
    public async Task An_allocation_beyond_that_sales_own_balance_is_rejected()
    {
        using var harness = new TestHarness();
        var (older, _) = await TwoDebtsAsync(harness);

        // 200 is under the client's 300, so only the per-sale check can catch this.
        var error = await Assert.ThrowsAsync<DomainRuleException>(() => harness.Sales.CollectFromClientAsync(
            new CollectDebtRequest(1, 200m, null, [new CollectAllocationRequest(older, 200m)])));

        Assert.Equal(ErrorCodes.PaymentExceedsBalance, error.ErrorCode);

        Assert.Empty(await harness.Db.PaymentReceipts.ToListAsync());
        Assert.Equal(100m, (await harness.Sales.GetAsync(older)).Balance);
    }

    [Fact]
    public async Task An_allocation_naming_a_sale_that_no_longer_owes_is_rejected()
    {
        using var harness = new TestHarness();
        var (older, _) = await TwoDebtsAsync(harness);

        await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 100m, null));

        // What a till holding a stale list would send: the sale another till has already settled.
        await Assert.ThrowsAsync<NotFoundException>(() => harness.Sales.CollectFromClientAsync(
            new CollectDebtRequest(1, 100m, null, [new CollectAllocationRequest(older, 100m)])));

        Assert.Single(await harness.Db.PaymentReceipts.ToListAsync());
    }

    [Fact]
    public async Task A_credit_sale_made_at_one_branch_can_be_collected_at_another()
    {
        using var harness = new TestHarness();
        var (older, _) = await TwoDebtsAsync(harness);

        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;

        var receipt = await harness.Sales.CollectFromClientAsync(new CollectDebtRequest(1, 100m, null));

        // The till that took the money is the one that has to balance.
        Assert.Equal(TestHarness.SouthBranchId, receipt.BranchId);
        Assert.Equal("SUR-000001", receipt.Number);
        Assert.Equal(older, receipt.Allocations.Single().SaleId);

        var payment = Assert.Single(await harness.Db.Payments.Where(p => p.ReceiptId == receipt.ReceiptId).ToListAsync());
        Assert.Equal(TestHarness.SouthBranchId, payment.BranchId);
    }
}
