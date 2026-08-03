using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

/// <summary>
/// The pick rule end to end: goods in, goods out, and the coexistence with stock counted before
/// tracking was switched on. The race these paths guard against — two tills claiming one unit — is
/// not covered here; the harness runs NoOpStockLock on SQLite.
/// </summary>
public class SerializedInventoryTests
{
    private static async Task<long> TrackedProductAsync(TestHarness harness)
    {
        var productId = await harness.CreateProductAsync(serialized: true);
        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(150.80m, 130m));
        return productId;
    }

    private static Task BuyAsync(TestHarness harness, long productId, int quantity, params string[] serials) =>
        harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, quantity, UnitPrice: 100m, serials.Length == 0 ? null : serials)]));

    private static Task<SaleDto> SellAsync(TestHarness harness, long productId, int quantity, params string[] serials) =>
        harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, quantity, serials.Length == 0 ? null : serials)]));

    /* ---------------------------------------------------------------- goods in */

    [Fact]
    public async Task Goods_arriving_for_a_tracked_product_need_one_serial_per_unit()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);

        await BuyAsync(harness, productId, 3, "SN-1", "SN-2", "SN-3");

        Assert.Equal(3, await harness.SerialCountAsync(productId, TestHarness.MainBranchId));
        Assert.Equal(["SN-1", "SN-2", "SN-3"], await harness.InStockSerialsAsync(productId, TestHarness.MainBranchId));
    }

    [Fact]
    public async Task An_inbound_line_whose_serials_do_not_match_its_quantity_is_refused()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            BuyAsync(harness, productId, 3, "SN-1", "SN-2"));

        Assert.Equal(ErrorCodes.SerialCountMismatch, exception.ErrorCode);
        // The whole purchase rolls back, stock included.
        Assert.Equal(0, (await harness.Products.GetAsync(productId)).AvailableQuantity);
    }

    [Fact]
    public async Task A_tracked_product_cannot_be_bought_without_serials_at_all()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => BuyAsync(harness, productId, 2));

        Assert.Equal(ErrorCodes.SerialCountMismatch, exception.ErrorCode);
    }

    [Fact]
    public async Task Serials_sent_for_an_untracked_product_are_refused()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            BuyAsync(harness, productId, 1, "SN-1"));

        Assert.Equal(ErrorCodes.SerialsNotTracked, exception.ErrorCode);
    }

    [Fact]
    public async Task One_serial_cannot_be_claimed_by_two_lines_of_the_same_document()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Purchases.CreateAsync(new CreatePurchaseRequest(
                harness.Today,
                SupplierId: 1,
                InvoiceType.WithInvoice,
                PaymentStatus.Paid,
                [
                    new PurchaseItemRequest(productId, 1, 100m, ["SN-1"]),
                    new PurchaseItemRequest(productId, 1, 120m, ["SN-1"])
                ])));

        Assert.Equal(ErrorCodes.DuplicateSerialNumber, exception.ErrorCode);
    }

    [Fact]
    public async Task A_unit_bought_before_stays_tied_to_the_line_that_brought_it_in()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");

        var purchase = await harness.Purchases.ListAsync(new PurchaseQuery(null, null, null));
        var detail = await harness.Purchases.GetAsync(purchase.Items[0].Id);

        var line = Assert.Single(detail.Items);
        Assert.Equal(["SN-1", "SN-2"], line.SerialNumbers);
    }

    /* ------------------------------------------------------- the pick rule */

    [Fact]
    public async Task Stock_counted_before_tracking_began_still_sells_with_no_serial()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        // Five anonymous units, then the shop adopts tracking and buys three named ones.
        await BuyAsync(harness, productId, 5);
        var product = await harness.Products.GetAsync(productId);
        await harness.Products.UpdateAsync(productId, new ProductRequest(
            product.Name, product.PartNumber, product.Description, IsSerialized: true, null, null, null));
        await harness.Products.SetPricesAsync(productId, new SetPricesRequest(150.80m, 130m));
        await BuyAsync(harness, productId, 3, "SN-1", "SN-2", "SN-3");

        // T=8, S=3, so four units may go out unidentified.
        var sale = await SellAsync(harness, productId, 4);

        Assert.Empty(Assert.Single(sale.Items).SerialNumbers);
        Assert.Equal(3, await harness.SerialCountAsync(productId, TestHarness.MainBranchId));

        // T=4, S=3, so only one anonymous unit is left: selling four now needs at least three names.
        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            SellAsync(harness, productId, 4, "SN-1"));
        Assert.Equal(ErrorCodes.SerialSelectionRequired, exception.ErrorCode);

        var settled = await SellAsync(harness, productId, 4, "SN-1", "SN-2", "SN-3");
        Assert.Equal(["SN-1", "SN-2", "SN-3"], Assert.Single(settled.Items).SerialNumbers);
        Assert.Equal(0, (await harness.Products.GetAsync(productId)).AvailableQuantity);
    }

    [Fact]
    public async Task Once_every_unit_is_named_a_sale_must_name_them_all()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 3, "SN-1", "SN-2", "SN-3");

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            SellAsync(harness, productId, 2, "SN-1"));

        Assert.Equal(ErrorCodes.SerialSelectionRequired, exception.ErrorCode);
        // The message has to carry the numbers, or a seller cannot act on it.
        Assert.Contains("at least 2", exception.Message);

        var sale = await SellAsync(harness, productId, 2, "SN-1", "SN-2");
        Assert.Equal(["SN-1", "SN-2"], Assert.Single(sale.Items).SerialNumbers);
    }

    [Fact]
    public async Task More_serials_than_units_is_refused()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 3, "SN-1", "SN-2", "SN-3");

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            SellAsync(harness, productId, 1, "SN-1", "SN-2"));

        Assert.Equal(ErrorCodes.SerialCountMismatch, exception.ErrorCode);
    }

    [Fact]
    public async Task A_unit_standing_in_another_branch_cannot_be_sold_here()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");

        await harness.Transfers.CreateAsync(new CreateTransferRequest(
            harness.Today, TestHarness.MainBranchId, TestHarness.SouthBranchId, null,
            [new TransferItemRequest(productId, 1, ["SN-1"])]));

        // SN-2 is still here, so there is stock to sell — only this particular unit has left.
        var exception = await Assert.ThrowsAsync<ConflictException>(() => SellAsync(harness, productId, 1, "SN-1"));

        Assert.Equal(ErrorCodes.SerialNotAvailable, exception.ErrorCode);
    }

    [Fact]
    public async Task A_unit_already_sold_cannot_be_sold_again()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");
        await SellAsync(harness, productId, 1, "SN-1");

        var exception = await Assert.ThrowsAsync<ConflictException>(() => SellAsync(harness, productId, 1, "SN-1"));

        Assert.Equal(ErrorCodes.SerialNotAvailable, exception.ErrorCode);
    }

    [Fact]
    public async Task Two_lines_of_one_product_keep_their_own_units_but_move_stock_once()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");

        var sale = await harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today, ClientId: 1, InvoiceType.WithoutInvoice, PaymentStatus.Paid, null,
            [
                new SaleItemRequest(productId, 1, ["SN-1"]),
                new SaleItemRequest(productId, 1, ["SN-2"])
            ]));

        Assert.Equal(2, sale.Items.Count);
        Assert.Equal(["SN-1"], sale.Items[0].SerialNumbers);
        Assert.Equal(["SN-2"], sale.Items[1].SerialNumbers);

        var movement = await harness.Db.InventoryMovements
            .SingleAsync(m => m.ProductId == productId && m.MovementType == MovementType.Sale);
        Assert.Equal(-2, movement.QuantityDelta);
    }

    [Fact]
    public async Task A_sale_rejected_for_its_serials_burns_no_folio()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");

        await Assert.ThrowsAsync<ConflictException>(() => SellAsync(harness, productId, 2, "SN-1"));

        var sale = await SellAsync(harness, productId, 2, "SN-1", "SN-2");
        Assert.EndsWith("000001", sale.Number);
    }

    /* ------------------------------------------------------ the warranty desk */

    [Fact]
    public async Task The_printed_warranty_note_carries_the_serial_of_every_unit_sold()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");
        var sale = await SellAsync(harness, productId, 2, "SN-1", "SN-2");

        var note = await harness.Reports.GetWarrantyNoteAsync(sale.Id);

        Assert.Equal(["SN-1", "SN-2"], Assert.Single(note.Sale.Items).SerialNumbers);
    }

    [Fact]
    public async Task Looking_a_sold_unit_up_names_the_sale_and_the_customer()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 1, "SN-1");
        var sale = await SellAsync(harness, productId, 1, "SN-1");

        var found = await harness.Serials.LookupAsync("SN-1");

        Assert.Equal(ProductSerialStatus.Sold, found.Status);
        Assert.NotNull(found.Sale);
        Assert.Equal(sale.Id, found.Sale.SaleId);
        Assert.Equal(sale.Number, found.Sale.Number);
        Assert.Equal("Juan Perez", found.Sale.ClientName);
    }

    [Fact]
    public async Task A_discontinued_product_still_answers_for_its_warranty()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 1, "SN-1");
        await SellAsync(harness, productId, 1, "SN-1");

        // The shop stops carrying the model; the units it sold are still under warranty.
        await harness.Products.DeleteAsync(productId);

        var found = await harness.Serials.LookupAsync("SN-1");

        Assert.Equal("SSD 1TB", found.ProductName);
        Assert.NotNull(found.Sale);
    }

    /* ----------------------------------------------------------- transfers */

    [Fact]
    public async Task A_transferred_unit_stands_in_the_destination_and_stays_sellable()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");

        var transfer = await harness.Transfers.CreateAsync(new CreateTransferRequest(
            harness.Today, TestHarness.MainBranchId, TestHarness.SouthBranchId, null,
            [new TransferItemRequest(productId, 1, ["SN-1"])]));

        Assert.Equal(["SN-2"], await harness.InStockSerialsAsync(productId, TestHarness.MainBranchId));
        Assert.Equal(["SN-1"], await harness.InStockSerialsAsync(productId, TestHarness.SouthBranchId));

        // The note has to say which unit travelled.
        Assert.Equal(["SN-1"], Assert.Single(transfer.Items).SerialNumbers);
    }

    [Fact]
    public async Task Anonymous_stock_still_transfers_without_naming_anything()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();
        await BuyAsync(harness, productId, 4);

        var product = await harness.Products.GetAsync(productId);
        await harness.Products.UpdateAsync(productId, new ProductRequest(
            product.Name, product.PartNumber, product.Description, IsSerialized: true, null, null, null));

        var transfer = await harness.Transfers.CreateAsync(new CreateTransferRequest(
            harness.Today, TestHarness.MainBranchId, TestHarness.SouthBranchId, null,
            [new TransferItemRequest(productId, 2)]));

        Assert.Empty(Assert.Single(transfer.Items).SerialNumbers);
    }

    /* ---------------------------------------------------------- adjustments */

    [Fact]
    public async Task Writing_a_unit_off_removes_it_by_name()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);
        await BuyAsync(harness, productId, 2, "SN-1", "SN-2");

        await harness.Inventory.AdjustAsync(
            new StockAdjustmentRequest(productId, -1, "damaged in transit", null, ["SN-1"]));

        Assert.Equal(["SN-2"], await harness.InStockSerialsAsync(productId, TestHarness.MainBranchId));

        // Removed, not deleted: the number stays spent so it can never be re-registered.
        var written = await harness.Serials.LookupAsync("SN-1");
        Assert.Equal(ProductSerialStatus.Removed, written.Status);
    }

    [Fact]
    public async Task A_correction_that_adds_stock_must_name_what_it_adds()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            harness.Inventory.AdjustAsync(new StockAdjustmentRequest(productId, 2, "found in the back")));

        Assert.Equal(ErrorCodes.SerialCountMismatch, exception.ErrorCode);
    }

    /* ------------------------------------------------------------- the stock list */

    /// <summary>
    /// The stock row has to say how much of what it holds can be named, or the screen cannot offer
    /// the serials without a call per product — and cannot tell a tracked product mid-adoption from
    /// an untracked one.
    /// </summary>
    [Fact]
    public async Task A_stock_row_reports_how_many_of_its_units_carry_a_serial()
    {
        using var harness = new TestHarness();
        var tracked = await harness.CreateProductAsync();
        var plain = await harness.CreateProductAsync("Cable HDMI");

        // One unit was on the shelf before tracking began, then the shop adopts it and buys two
        // named ones: mid-adoption, which is exactly when the two numbers differ.
        await BuyAsync(harness, tracked, 1);
        var product = await harness.Products.GetAsync(tracked);
        await harness.Products.UpdateAsync(tracked, new ProductRequest(
            product.Name, product.PartNumber, product.Description, IsSerialized: true, null, null, null));
        await BuyAsync(harness, tracked, 2, "SN-1", "SN-2");
        await BuyAsync(harness, plain, 5);

        var rows = (await harness.Inventory.ListStockAsync(new StockQuery(null))).Items;

        var trackedRow = Assert.Single(rows, r => r.ProductId == tracked);
        Assert.True(trackedRow.IsSerialized);
        Assert.Equal(3, trackedRow.Quantity);
        Assert.Equal(2, trackedRow.SerializedQuantity);

        var plainRow = Assert.Single(rows, r => r.ProductId == plain);
        Assert.False(plainRow.IsSerialized);
        Assert.Equal(0, plainRow.SerializedQuantity);
    }

    /* ------------------------------------------------------------ invariant */

    [Fact]
    public async Task No_branch_ever_holds_more_named_units_than_stock()
    {
        using var harness = new TestHarness();
        var productId = await TrackedProductAsync(harness);

        await BuyAsync(harness, productId, 4, "SN-1", "SN-2", "SN-3", "SN-4");
        await harness.Transfers.CreateAsync(new CreateTransferRequest(
            harness.Today, TestHarness.MainBranchId, TestHarness.SouthBranchId, null,
            [new TransferItemRequest(productId, 1, ["SN-1"])]));
        await SellAsync(harness, productId, 1, "SN-2");
        await harness.Inventory.AdjustAsync(new StockAdjustmentRequest(productId, -1, "lost", null, ["SN-3"]));

        Assert.Empty(await harness.Serials.GetDriftAsync());
    }
}
