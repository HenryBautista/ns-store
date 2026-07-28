using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Features.Branches;
using NsStore.Domain.Common;

namespace NsStore.Application.Tests;

public class BranchServiceTests
{
    [Fact]
    public async Task Creating_a_branch_fans_out_a_stock_row_for_every_live_product()
    {
        using var harness = new TestHarness();
        var first = await harness.CreateProductAsync("SSD 1TB");
        var second = await harness.CreateProductAsync("RAM 16GB");

        var branch = await harness.Branches.CreateAsync(new BranchRequest("NORTE", "Sucursal Norte", null, null));

        var rows = await harness.Db.StockLevels.Where(s => s.BranchId == branch.Id).ToListAsync();

        // The dense grid is a correctness invariant, not tidiness: SELECT … FOR UPDATE only locks
        // rows that exist, so a missing row silently disables the oversell guard.
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal(0, r.Quantity));
        Assert.Contains(rows, r => r.ProductId == first);
        Assert.Contains(rows, r => r.ProductId == second);
    }

    [Fact]
    public async Task Creating_a_product_fans_out_a_stock_row_for_every_active_branch()
    {
        using var harness = new TestHarness();

        var productId = await harness.CreateProductAsync();

        var branchIds = await harness.Db.StockLevels
            .Where(s => s.ProductId == productId)
            .Select(s => s.BranchId)
            .ToListAsync();

        Assert.Equal(
            [TestHarness.MainBranchId, TestHarness.SouthBranchId],
            branchIds.Order());
    }

    [Fact]
    public async Task The_code_is_stored_uppercase_and_must_be_unique_regardless_of_case()
    {
        using var harness = new TestHarness();

        var branch = await harness.Branches.CreateAsync(new BranchRequest("norte", "Sucursal Norte", null, null));
        Assert.Equal("NORTE", branch.Code);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            harness.Branches.CreateAsync(new BranchRequest("Norte", "Otra", null, null)));

        Assert.Equal(ErrorCodes.DuplicateBranchCode, exception.ErrorCode);
    }

    [Fact]
    public async Task Deleting_a_branch_that_still_holds_stock_is_rejected()
    {
        using var harness = new TestHarness();
        var productId = await harness.CreateProductAsync();

        var stock = await harness.Db.StockLevels
            .SingleAsync(s => s.BranchId == TestHarness.SouthBranchId && s.ProductId == productId);
        stock.Quantity = 3;
        await harness.Db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => harness.Branches.DeleteAsync(TestHarness.SouthBranchId));
    }

    [Fact]
    public async Task Deleting_a_branch_that_has_users_assigned_is_rejected()
    {
        using var harness = new TestHarness();

        // The seeded admin lives in MAIN.
        await Assert.ThrowsAsync<ConflictException>(() => harness.Branches.DeleteAsync(TestHarness.MainBranchId));
    }

    [Fact]
    public async Task An_empty_branch_can_be_soft_deleted()
    {
        using var harness = new TestHarness();

        await harness.Branches.DeleteAsync(TestHarness.SouthBranchId);

        var branch = await harness.Db.Branches.SingleAsync(b => b.Id == TestHarness.SouthBranchId);
        Assert.NotNull(branch.DeletedAt);
    }

    [Fact]
    public void FormatDocumentNumber_pads_the_sequence_to_six_digits()
    {
        var branch = new Domain.Entities.Branch { Code = "MAIN", Name = "Casa Matriz" };

        Assert.Equal("MAIN-000001", branch.FormatDocumentNumber(1));
        Assert.Equal("MAIN-000123", branch.FormatDocumentNumber(123));
        Assert.Equal("MAIN-1234567", branch.FormatDocumentNumber(1234567));
    }
}
