using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Products;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// Covers naming units that were already counted, and looking a unit up at the warranty desk.
/// The concurrency behaviour these paths rely on — two tills racing for the same serial — is not
/// exercised here: the harness uses NoOpStockLock on SQLite, so only PostgreSQL enforces it.
/// </summary>
public class SerialServiceTests
{
    /// <summary>
    /// The state a shop is actually in when it adopts tracking: stock already on the shelf, none of
    /// it identified.
    /// </summary>
    private static async Task<long> ProductWithUntrackedStockAsync(TestHarness harness, int quantity)
    {
        var productId = await harness.CreateProductAsync();
        await harness.Inventory.AdjustAsync(new StockAdjustmentRequest(productId, quantity, "opening count"));

        var product = await harness.Products.GetAsync(productId);
        await harness.Products.UpdateAsync(productId, new ProductRequest(
            product.Name, product.PartNumber, product.Description, IsSerialized: true, null, null, null));

        return productId;
    }

    [Fact]
    public async Task Turning_tracking_on_never_demands_a_stock_count()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 10);

        var product = await harness.Products.GetAsync(productId);

        Assert.True(product.IsSerialized);
        Assert.Equal(10, product.AvailableQuantity);
        // Ten units on the shelf, none of them named, and nothing was asked of anyone.
        Assert.Equal(0, product.SerializedQuantity);
    }

    [Fact]
    public async Task Registering_names_units_already_on_the_shelf_without_moving_stock()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 3);

        var created = await harness.Serials.RegisterAsync(
            new RegisterSerialsRequest(productId, ["SN-1", "SN-2"]));

        Assert.Equal(2, created.Count);
        Assert.All(created, s => Assert.Equal(ProductSerialStatus.InStock, s.Status));

        var product = await harness.Products.GetAsync(productId);
        Assert.Equal(3, product.AvailableQuantity);
        Assert.Equal(2, product.SerializedQuantity);

        // Nothing arrived, so nothing may appear in the ledger.
        Assert.Empty(await harness.Db.InventoryMovements
            .Where(m => m.ProductId == productId && m.MovementType == MovementType.Adjustment && m.QuantityDelta > 0)
            .Skip(1)
            .ToListAsync());
    }

    [Fact]
    public async Task Registering_cannot_name_more_units_than_the_branch_holds()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 2);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["SN-1", "SN-2", "SN-3"])));

        Assert.Equal(ErrorCodes.SerialStockExceeded, exception.ErrorCode);
        Assert.Equal(0, await harness.SerialCountAsync(productId, TestHarness.MainBranchId));
    }

    [Fact]
    public async Task A_serial_belongs_to_one_unit_across_the_whole_system()
    {
        using var harness = new TestHarness();
        var first = await ProductWithUntrackedStockAsync(harness, 2);
        var second = await ProductWithUntrackedStockAsync(harness, 2);

        await harness.Serials.RegisterAsync(new RegisterSerialsRequest(first, ["SN-1"]));

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Serials.RegisterAsync(new RegisterSerialsRequest(second, ["SN-1"])));

        Assert.Equal(ErrorCodes.DuplicateSerialNumber, exception.ErrorCode);
    }

    [Fact]
    public async Task A_serial_collides_with_itself_in_another_casing()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 4);

        await harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["AB123"]));

        // The lower() index is PostgreSQL-only, so on SQLite this proves the service check itself.
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["ab123"])));

        Assert.Equal(ErrorCodes.DuplicateSerialNumber, exception.ErrorCode);
    }

    [Fact]
    public async Task The_same_serial_twice_in_one_request_is_rejected()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 4);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["SN-1", "sn-1"])));

        Assert.Equal(ErrorCodes.DuplicateSerialNumber, exception.ErrorCode);
    }

    [Fact]
    public async Task Registering_is_refused_for_a_product_that_is_not_tracked()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();
        await harness.Inventory.AdjustAsync(new StockAdjustmentRequest(productId, 5, null));

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["SN-1"])));

        Assert.Equal(ErrorCodes.SerialsNotTracked, exception.ErrorCode);
    }

    [Fact]
    public async Task Casing_and_padding_do_not_stop_the_warranty_desk_finding_a_unit()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 2);
        await harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["AB123"]));

        var found = await harness.Serials.LookupAsync("  ab123 ");

        // Stored as typed, so it still matches the sticker on the box.
        Assert.Equal("AB123", found.SerialNumber);
        Assert.Equal(productId, found.ProductId);
        Assert.Equal(ProductSerialStatus.InStock, found.Status);
        // Never sold, so there is no sale to point at.
        Assert.Null(found.Sale);
    }

    [Fact]
    public async Task A_serial_we_never_issued_is_not_found()
    {
        using var harness = new TestHarness();
        await ProductWithUntrackedStockAsync(harness, 2);

        await Assert.ThrowsAsync<NotFoundException>(() => harness.Serials.LookupAsync("NOT-OURS-9999"));
    }

    [Fact]
    public async Task Listing_shows_only_the_branch_asked_about()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 3);
        await harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["SN-1", "SN-2"]));

        var here = await harness.Serials.ListAsync(new SerialQuery(productId, TestHarness.MainBranchId));
        var elsewhere = await harness.Serials.ListAsync(new SerialQuery(productId, TestHarness.SouthBranchId));

        Assert.Equal(2, here.Items.Count);
        Assert.Empty(elsewhere.Items);
    }

    [Fact]
    public async Task Tracking_cannot_be_switched_off_while_named_units_are_in_stock()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 3);
        await harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["SN-1"]));

        var product = await harness.Products.GetAsync(productId);
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Products.UpdateAsync(productId, new ProductRequest(
                product.Name, product.PartNumber, product.Description, IsSerialized: false, null, null, null)));

        Assert.Equal(ErrorCodes.SerializationInUse, exception.ErrorCode);
    }

    [Fact]
    public async Task Registering_writes_the_units_history()
    {
        using var harness = new TestHarness();
        var productId = await ProductWithUntrackedStockAsync(harness, 2);
        var created = await harness.Serials.RegisterAsync(new RegisterSerialsRequest(productId, ["SN-1"]));

        var history = await harness.Serials.GetHistoryAsync(created[0].Id);

        var entry = Assert.Single(history);
        Assert.Equal(SerialEventType.Registered, entry.EventType);
        Assert.Equal("manual", entry.ReferenceType);
    }
}
