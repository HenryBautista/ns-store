using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

public class Sale : AuditableEntity
{
    public DateOnly SaleDate { get; set; }

    /// <summary>The branch that sold. Stamped from the caller's active branch, never from the body.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;
    public long ClientId { get; set; }
    public Client Client { get; set; } = null!;

    /// <summary>Applies to the whole sale — it selects which product price is used.</summary>
    public InvoiceType InvoiceType { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public List<SaleItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];

    public decimal Balance => TotalAmount - TotalPaid;

    /// <summary>
    /// Registers an installment, keeping <see cref="TotalPaid"/> and the status consistent.
    /// <paramref name="branchId"/> is the branch that <em>receives</em> the money, which may differ
    /// from the branch that made the sale — that is what balances a till.
    /// </summary>
    public Payment RegisterPayment(decimal amount, DateOnly paymentDate, long branchId, long? userId, DateTimeOffset now)
    {
        if (amount <= 0)
        {
            throw new DomainRuleException(ErrorCodes.ValidationError, "Payment amount must be greater than zero");
        }

        if (amount > Balance)
        {
            throw new DomainRuleException(
                ErrorCodes.PaymentExceedsBalance,
                $"Payment {amount} exceeds outstanding balance {Balance}");
        }

        var payment = new Payment
        {
            SaleId = Id,
            BranchId = branchId,
            Amount = amount,
            PaymentDate = paymentDate,
            CreatedBy = userId,
            CreatedAt = now
        };

        Payments.Add(payment);
        TotalPaid += amount;
        if (Balance == 0)
        {
            PaymentStatus = PaymentStatus.Paid;
        }

        return payment;
    }
}

public class SaleItem
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }

    /// <summary>Unit price resolved from the sale's <see cref="InvoiceType"/> at sale time.</summary>
    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }
}

/// <summary>Installment ("abono") against a credit sale — traceability the legacy lacked.</summary>
public class Payment
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    /// <summary>The branch that received the money — the till it has to balance against.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
