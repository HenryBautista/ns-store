using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Tests;

public class ProductSerialTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero);

    private static ProductSerial InStock(long branchId = 1) => new()
    {
        ProductId = 1,
        SerialNumber = "SN-0001",
        BranchId = branchId,
        Status = ProductSerialStatus.InStock
    };

    [Fact]
    public void MarkSold_binds_the_unit_to_the_line_it_left_on()
    {
        var serial = InStock();
        var line = new SaleItem { Id = 7 };

        serial.MarkSold(line, Now);

        Assert.Equal(ProductSerialStatus.Sold, serial.Status);
        Assert.Same(line, serial.SaleItem);
        Assert.Equal(Now, serial.SoldAt);
        Assert.Equal(1, serial.Version);
    }

    [Fact]
    public void A_unit_cannot_be_sold_twice()
    {
        var serial = InStock();
        serial.MarkSold(new SaleItem { Id = 7 }, Now);

        var exception = Assert.Throws<DomainRuleException>(() => serial.MarkSold(new SaleItem { Id = 8 }, Now));

        Assert.Equal(ErrorCodes.SerialNotAvailable, exception.ErrorCode);
        // The first sale still owns it: a rejected second sale must not rewrite the evidence.
        Assert.Equal(7, serial.SaleItem!.Id);
    }

    [Fact]
    public void MarkTransferred_moves_the_unit_but_leaves_it_in_stock()
    {
        var serial = InStock(branchId: 1);

        serial.MarkTransferred(destinationBranchId: 2, Now);

        Assert.Equal(2, serial.BranchId);
        Assert.Equal(ProductSerialStatus.InStock, serial.Status);
    }

    [Fact]
    public void MarkTransferred_rejects_the_branch_the_unit_is_already_in()
    {
        var serial = InStock(branchId: 1);

        var exception = Assert.Throws<DomainRuleException>(() => serial.MarkTransferred(1, Now));

        Assert.Equal(ErrorCodes.SerialNotAvailable, exception.ErrorCode);
    }

    [Fact]
    public void A_sold_unit_can_neither_be_transferred_nor_removed()
    {
        var serial = InStock();
        serial.MarkSold(new SaleItem { Id = 7 }, Now);

        Assert.Throws<DomainRuleException>(() => serial.MarkTransferred(2, Now));
        Assert.Throws<DomainRuleException>(() => serial.MarkRemoved(Now));
        Assert.Equal(ProductSerialStatus.Sold, serial.Status);
    }

    [Fact]
    public void MarkRemoved_writes_the_unit_off()
    {
        var serial = InStock();

        serial.MarkRemoved(Now);

        Assert.Equal(ProductSerialStatus.Removed, serial.Status);
        Assert.Null(serial.SoldAt);
    }

    [Fact]
    public void A_removed_unit_cannot_come_back_by_being_sold()
    {
        var serial = InStock();
        serial.MarkRemoved(Now);

        var exception = Assert.Throws<DomainRuleException>(() => serial.MarkSold(new SaleItem { Id = 7 }, Now));

        Assert.Equal(ErrorCodes.SerialNotAvailable, exception.ErrorCode);
    }
}
