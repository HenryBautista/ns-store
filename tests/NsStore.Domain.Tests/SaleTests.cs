using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Tests;

public class SaleTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 7, 24);

    private static Sale CreditSale(decimal total, decimal paid = 0m) => new()
    {
        Id = 1,
        TotalAmount = total,
        TotalPaid = paid,
        PaymentStatus = PaymentStatus.Credit
    };

    [Fact]
    public void RegisterPayment_reduces_the_balance()
    {
        var sale = CreditSale(1000m, 200m);

        sale.RegisterPayment(300m, Today, userId: 7, Now);

        Assert.Equal(500m, sale.TotalPaid);
        Assert.Equal(500m, sale.Balance);
        Assert.Equal(PaymentStatus.Credit, sale.PaymentStatus);
        Assert.Single(sale.Payments);
    }

    [Fact]
    public void RegisterPayment_settles_the_sale_when_the_balance_reaches_zero()
    {
        var sale = CreditSale(1000m, 400m);

        sale.RegisterPayment(600m, Today, userId: 7, Now);

        Assert.Equal(0m, sale.Balance);
        Assert.Equal(PaymentStatus.Paid, sale.PaymentStatus);
    }

    [Fact]
    public void RegisterPayment_rejects_an_amount_above_the_balance()
    {
        var sale = CreditSale(1000m, 900m);

        var exception = Assert.Throws<DomainRuleException>(() => sale.RegisterPayment(200m, Today, null, Now));

        Assert.Equal(ErrorCodes.PaymentExceedsBalance, exception.ErrorCode);
        Assert.Equal(900m, sale.TotalPaid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void RegisterPayment_rejects_non_positive_amounts(decimal amount)
    {
        var sale = CreditSale(1000m);

        Assert.Throws<DomainRuleException>(() => sale.RegisterPayment(amount, Today, null, Now));
    }
}

public class OrderTests
{
    [Fact]
    public void EnsureAdvanceWithinPrice_rejects_an_advance_above_the_price()
    {
        var order = new Order { Price = 500m, AdvanceAmount = 600m };

        var exception = Assert.Throws<DomainRuleException>(order.EnsureAdvanceWithinPrice);

        Assert.Equal(ErrorCodes.AdvanceExceedsPrice, exception.ErrorCode);
    }

    [Fact]
    public void Balance_is_price_minus_advance()
    {
        var order = new Order { Price = 500m, AdvanceAmount = 120m };

        order.EnsureAdvanceWithinPrice();

        Assert.Equal(380m, order.Balance);
    }
}

public class ProductTests
{
    [Theory]
    [InlineData(InvoiceType.WithInvoice, 116)]
    [InlineData(InvoiceType.WithoutInvoice, 100)]
    public void PriceFor_selects_the_price_matching_the_invoice_type(InvoiceType invoiceType, decimal expected)
    {
        var product = new Product { Name = "SSD", PriceWithInvoice = 116m, PriceWithoutInvoice = 100m };

        Assert.Equal(expected, product.PriceFor(invoiceType));
    }
}
