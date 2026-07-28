using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Application.Tests;

public class DocumentNumberingTests
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

    private static Task<SaleDto> SellAsync(TestHarness harness, long productId, int quantity = 1) =>
        harness.Sales.CreateAsync(new CreateSaleRequest(
            harness.Today,
            ClientId: 1,
            InvoiceType.WithoutInvoice,
            PaymentStatus.Paid,
            InitialPaid: null,
            [new SaleItemRequest(productId, quantity)]));

    [Fact]
    public async Task The_first_sale_of_a_branch_is_numbered_one()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await SellAsync(harness, productId);

        Assert.Equal("MAIN-000001", sale.Number);
    }

    [Fact]
    public async Task Numbers_increment_within_a_branch()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var first = await SellAsync(harness, productId);
        var second = await SellAsync(harness, productId);
        var third = await SellAsync(harness, productId);

        Assert.Equal(["MAIN-000001", "MAIN-000002", "MAIN-000003"], new[] { first.Number, second.Number, third.Number });
        Assert.Equal(3, third.BranchSequenceOrDefault());
    }

    [Fact]
    public async Task Each_branch_keeps_its_own_series()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var mainSale = await SellAsync(harness, productId);

        // Move the admin to SUR and stock it there, so a sale is possible.
        harness.CurrentUser.ActiveBranchId = TestHarness.SouthBranchId;
        await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today, 1, InvoiceType.WithInvoice, PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, 5, 100m)]));

        var southSale = await SellAsync(harness, productId);

        // Both are the first of their own branch: the series are independent, not shared.
        Assert.Equal("MAIN-000001", mainSale.Number);
        Assert.Equal("SUR-000001", southSale.Number);
    }

    [Fact]
    public async Task Sales_and_purchases_count_separately()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness);

        var sale = await SellAsync(harness, productId);
        var purchase = await harness.Purchases.CreateAsync(new CreatePurchaseRequest(
            harness.Today, 1, InvoiceType.WithInvoice, PaymentStatus.Paid,
            [new PurchaseItemRequest(productId, 1, 100m)]));

        Assert.Equal("MAIN-000001", sale.Number);
        // ReadyProductAsync already made one purchase, so this is the branch's second.
        Assert.Equal("MAIN-000002", purchase.Number);
    }

    /// <summary>
    /// The entire justification for the counter-column design over a Postgres sequence. Sequences
    /// are not transactional, so a rolled-back sale would burn its number forever — and gaps in a
    /// series of fiscal documents are exactly what this has to avoid.
    /// </summary>
    [Fact]
    public async Task A_failed_sale_does_not_burn_a_number()
    {
        using var harness = new TestHarness();
        var productId = await ReadyProductAsync(harness, quantity: 3);

        var first = await SellAsync(harness, productId);
        Assert.Equal("MAIN-000001", first.Number);

        var exception = await Assert.ThrowsAsync<DomainRuleException>(() => SellAsync(harness, productId, quantity: 99));
        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);

        harness.Db.ChangeTracker.Clear();
        var next = await SellAsync(harness, productId);

        Assert.Equal("MAIN-000002", next.Number);

        var branch = await harness.Db.Branches.AsNoTracking().SingleAsync(b => b.Id == TestHarness.MainBranchId);
        Assert.Equal(2, branch.SaleSequence);
    }

    [Fact]
    public async Task The_service_hands_out_consecutive_numbers_per_kind()
    {
        using var harness = new TestHarness();

        var a = await harness.DocumentNumbers.NextAsync(TestHarness.MainBranchId, DocumentKind.Transfer);
        var b = await harness.DocumentNumbers.NextAsync(TestHarness.MainBranchId, DocumentKind.Transfer);
        var other = await harness.DocumentNumbers.NextAsync(TestHarness.SouthBranchId, DocumentKind.Transfer);

        Assert.Equal(1, a);
        Assert.Equal(2, b);
        Assert.Equal(1, other);
    }
}

internal static class SaleDtoAssertions
{
    /// <summary>The DTO carries the rendered folio; the numeric part is what the suffix encodes.</summary>
    public static long BranchSequenceOrDefault(this SaleDto sale) =>
        long.TryParse(sale.Number.Split('-')[^1], out var value) ? value : 0;
}
