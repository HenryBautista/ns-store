using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

public class Sale : AuditableEntity
{
    public DateOnly SaleDate { get; set; }

    /// <summary>The branch that sold. Stamped from the caller's active branch, never from the body.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    /// <summary>Per-branch correlative — the numeric invariant behind <see cref="Number"/>.</summary>
    public long BranchSequence { get; set; }

    /// <summary>
    /// The rendered folio, e.g. <c>MAIN-000123</c>. Stored rather than derived so that renaming a
    /// branch code later does not rewrite the number already printed on the customer's copy.
    /// </summary>
    public string Number { get; set; } = null!;

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

/// <summary>
/// One act of collection: the customer hands over an amount, which is spread across whichever of
/// their sales still carry a balance. Exists so the paper the customer walks away with has a stable
/// identity — without it a collection is only a scatter of <see cref="Payment"/> rows that nothing
/// ties together, and a lost receipt could never be reissued.
/// </summary>
public class PaymentReceipt : AuditableEntity
{
    public long ClientId { get; set; }
    public Client Client { get; set; } = null!;

    /// <summary>The branch whose till took the money — not necessarily the one that sold.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    /// <summary>Per-branch correlative, same mechanism as a sale's.</summary>
    public long BranchSequence { get; set; }

    /// <summary>The rendered folio, e.g. <c>MAIN-000012</c>. Stored, so renaming a branch never rewrites it.</summary>
    public string Number { get; set; } = null!;

    public DateOnly ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<Payment> Payments { get; set; } = [];
}

/// <summary>Installment ("abono") against a credit sale — traceability the legacy lacked.</summary>
public class Payment
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    /// <summary>
    /// The collection this instalment belonged to. Nullable: payments taken one sale at a time
    /// (the POS initial payment, the per-sale collect screen) issue no receipt.
    /// </summary>
    public long? ReceiptId { get; set; }

    public PaymentReceipt? Receipt { get; set; }

    /// <summary>The branch that received the money — the till it has to balance against.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
