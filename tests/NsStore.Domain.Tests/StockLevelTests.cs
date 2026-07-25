using NsStore.Domain.Common;
using NsStore.Domain.Entities;

namespace NsStore.Domain.Tests;

public class StockLevelTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Apply_adds_inbound_quantity_and_bumps_version()
    {
        var stock = new StockLevel { ProductId = 1, Quantity = 5 };

        stock.Apply(3, Now);

        Assert.Equal(8, stock.Quantity);
        Assert.Equal(1, stock.Version);
        Assert.Equal(Now, stock.UpdatedAt);
    }

    [Fact]
    public void Apply_allows_selling_the_whole_quantity_and_keeps_the_row_at_zero()
    {
        var stock = new StockLevel { ProductId = 1, Quantity = 4 };

        stock.Apply(-4, Now);

        Assert.Equal(0, stock.Quantity);
    }

    [Fact]
    public void Apply_rejects_going_below_zero()
    {
        var stock = new StockLevel { ProductId = 1, Quantity = 2 };

        var exception = Assert.Throws<DomainRuleException>(() => stock.Apply(-3, Now));

        Assert.Equal(ErrorCodes.InsufficientStock, exception.ErrorCode);
        Assert.Equal(2, stock.Quantity);
    }
}
