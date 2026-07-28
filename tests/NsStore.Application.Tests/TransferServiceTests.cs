using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Purchases;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

public class TransferServiceTests
{
    private static async Task<long> StockedInMainAsync(TestHarness harness, int quantity = 10)
    {
        var productId = await harness.CreateProductAsync();

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, quantity, UnitPrice: 100m)]));

        return productId;
    }

    private static Task<TransferDto> TransferAsync(TestHarness harness, long productId, int quantity) =>
        harness.Transfers.CreateAsync(new CreateTransferRequest(
            harness.Today,
            OriginBranchId: TestHarness.MainBranchId,
            DestinationBranchId: TestHarness.SouthBranchId,
            Notes: null,
            [new TransferItemRequest(productId, quantity)]));

    private static Task<int> QuantityAsync(TestHarness harness, long branchId, long productId) =>
        harness.Db.StockLevels.AsNoTracking()
            .Where(s => s.BranchId == branchId && s.ProductId == productId)
            .Select(s => s.Quantity)
            .SingleAsync();

    [Fact]
    public async Task A_transfer_moves_both_rows_and_writes_exactly_two_movements()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        var transfer = await TransferAsync(harness, productId, 4);

        Assert.Equal(6, await QuantityAsync(harness, TestHarness.MainBranchId, productId));
        Assert.Equal(4, await QuantityAsync(harness, TestHarness.SouthBranchId, productId));
        Assert.Equal(4, transfer.TotalQuantity);

        var movements = await harness.Db.InventoryMovements.AsNoTracking()
            .Where(m => m.ReferenceType == "transfer" && m.ReferenceId == transfer.Id)
            .ToListAsync();

        Assert.Equal(2, movements.Count);

        var outbound = movements.Single(m => m.MovementType == MovementType.TransferOut);
        Assert.Equal(TestHarness.MainBranchId, outbound.BranchId);
        Assert.Equal(-4, outbound.QuantityDelta);

        var inbound = movements.Single(m => m.MovementType == MovementType.TransferIn);
        Assert.Equal(TestHarness.SouthBranchId, inbound.BranchId);
        Assert.Equal(4, inbound.QuantityDelta);
    }

    /// <summary>
    /// The heart of the design: origin and destination move together or not at all.
    /// </summary>
    [Fact]
    public async Task Insufficient_stock_at_the_origin_leaves_the_destination_untouched()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness, quantity: 3);

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => TransferAsync(harness, productId, 5));
        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);

        harness.Db.ChangeTracker.Clear();

        Assert.Equal(3, await QuantityAsync(harness, TestHarness.MainBranchId, productId));
        Assert.Equal(0, await QuantityAsync(harness, TestHarness.SouthBranchId, productId));
        Assert.Empty(await harness.Db.StockTransfers.AsNoTracking().ToListAsync());
        Assert.Empty(await harness.Db.InventoryMovements.AsNoTracking()
            .Where(m => m.ReferenceType == "transfer")
            .ToListAsync());
    }

    [Fact]
    public async Task Transferring_a_branch_to_itself_is_rejected()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() =>
            harness.Transfers.CreateAsync(new CreateTransferRequest(
                harness.Today,
                TestHarness.MainBranchId,
                TestHarness.MainBranchId,
                null,
                [new TransferItemRequest(productId, 1)])));

        Assert.Equal(ErrorCodes.SameBranchTransfer, exception.ErrorCode);
    }

    [Fact]
    public async Task A_seller_cannot_dispatch_from_a_branch_that_is_not_theirs()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        harness.CurrentUser.Role = UserRole.Seller;
        harness.CurrentUser.HomeBranchId = TestHarness.SouthBranchId;
        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;

        // The origin is the writing side, so dispatching out of MAIN is not theirs to do.
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => TransferAsync(harness, productId, 1));

        Assert.Equal(ErrorCodes.BranchNotAllowed, exception.ErrorCode);
    }

    [Fact]
    public async Task The_same_product_on_several_lines_moves_once()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        var transfer = await harness.Transfers.CreateAsync(new CreateTransferRequest(
            harness.Today,
            TestHarness.MainBranchId,
            TestHarness.SouthBranchId,
            null,
            [new TransferItemRequest(productId, 2), new TransferItemRequest(productId, 3)]));

        Assert.Single(transfer.Items);
        Assert.Equal(5, transfer.TotalQuantity);
        Assert.Equal(5, await QuantityAsync(harness, TestHarness.SouthBranchId, productId));

        var movements = await harness.Db.InventoryMovements.AsNoTracking()
            .Where(m => m.ReferenceType == "transfer" && m.ReferenceId == transfer.Id)
            .CountAsync();

        Assert.Equal(2, movements);
    }

    [Fact]
    public async Task Transfers_take_their_correlative_from_the_origin_branch()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        var first = await TransferAsync(harness, productId, 1);
        var second = await TransferAsync(harness, productId, 1);

        Assert.Equal("MAIN-000001", first.Number);
        Assert.Equal("MAIN-000002", second.Number);
    }

    [Fact]
    public async Task A_branch_list_shows_what_it_sent_and_what_it_received()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness);

        await TransferAsync(harness, productId, 2);

        var fromOrigin = await harness.Transfers.ListAsync(new TransferQuery(BranchId: TestHarness.MainBranchId));
        var fromDestination = await harness.Transfers.ListAsync(new TransferQuery(BranchId: TestHarness.SouthBranchId));

        Assert.Single(fromOrigin.Items);
        Assert.Single(fromDestination.Items);
    }

    /// <summary>The extended kardex identity has to keep balancing once transfers exist.</summary>
    [Fact]
    public async Task The_kardex_identity_still_holds_after_a_transfer()
    {
        using var harness = new TestHarness();
        var productId = await StockedInMainAsync(harness, quantity: 10);

        await TransferAsync(harness, productId, 4);

        foreach (var branchId in new[] { TestHarness.MainBranchId, TestHarness.SouthBranchId })
        {
            var page = await harness.Inventory.GetKardexAsync(new KardexQuery(null, branchId));
            var row = page.Items.Single(r => r.ProductId == productId);

            Assert.Equal(
                row.TotalPurchased - row.TotalSold + row.TotalAdjusted + row.TotalTransferredIn - row.TotalTransferredOut,
                row.Available);
        }
    }
}
