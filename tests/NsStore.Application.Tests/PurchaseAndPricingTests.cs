using Microsoft.EntityFrameworkCore;
using NsStore.Application.Features.Purchases;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

public class PurchaseAndPricingTests
{
    [Fact]
    public async Task Purchase_increments_stock_and_records_the_ledger_movement()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        var purchase = await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, Quantity: 10, UnitPrice: 100m)]));

        Assert.Equal(10, purchase.TotalQuantity);
        Assert.Equal(1000m, purchase.TotalAmount);

        var stock = await harness.Db.StockLevels
            .SingleAsync(s => s.ProductId == productId && s.BranchId == TestHarness.MainBranchId);
        Assert.Equal(10, stock.Quantity);

        var movement = await harness.Db.InventoryMovements.SingleAsync(m => m.ProductId == productId);
        Assert.Equal(MovementType.Purchase, movement.MovementType);
        Assert.Equal(10, movement.QuantityDelta);
        Assert.Equal(100m, movement.UnitCost);
        Assert.Equal(purchase.Id, movement.ReferenceId);
    }

    [Fact]
    public async Task Price_suggestion_applies_margin_then_vat_from_settings()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, Quantity: 5, UnitPrice: 100m)]));

        var suggestion = await harness.Products.GetPriceSuggestionAsync(productId);

        Assert.Equal(100m, suggestion.LastCost);
        Assert.Equal(130m, suggestion.SuggestedWithoutInvoice);   // 100 × 1.30
        Assert.Equal(150.80m, suggestion.SuggestedWithInvoice);   // 130 × 1.16
    }

    [Fact]
    public async Task Price_suggestion_is_empty_without_purchase_history()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        var suggestion = await harness.Products.GetPriceSuggestionAsync(productId);

        Assert.Null(suggestion.LastCost);
        Assert.Null(suggestion.SuggestedWithoutInvoice);
        Assert.Null(suggestion.SuggestedWithInvoice);
    }

    [Fact]
    public async Task Price_suggestion_follows_updated_settings()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();
        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today,
            SupplierId: 1,
            InvoiceType.WithInvoice,
            PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, Quantity: 1, UnitPrice: 200m)]));

        await harness.Settings.UpdateAsync(new Features.Settings.UpdateSettingsRequest(
            VatRate: 13m,
            DefaultMarginPct: 50m,
            Currency: "BOB"));

        var suggestion = await harness.Products.GetPriceSuggestionAsync(productId);

        Assert.Equal(300m, suggestion.SuggestedWithoutInvoice);   // 200 × 1.50
        Assert.Equal(339m, suggestion.SuggestedWithInvoice);      // 300 × 1.13
    }

    [Fact]
    public async Task Manual_adjustment_cannot_push_stock_below_zero()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() =>
            harness.Inventory.AdjustAsync(new Features.Inventory.StockAdjustmentRequest(productId, -1, "typo")));

        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);
    }
}
