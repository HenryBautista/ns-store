using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

public class Purchase : AuditableEntity
{
    public DateOnly PurchaseDate { get; set; }

    /// <summary>The branch the goods entered. Stamped from the caller's active branch.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    /// <summary>Per-branch correlative; see <c>Sale.BranchSequence</c>.</summary>
    public long BranchSequence { get; set; }

    public string Number { get; set; } = null!;

    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public InvoiceType InvoiceType { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseItem> Items { get; set; } = [];
}

public class PurchaseItem
{
    public long Id { get; set; }
    public long PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }

    /// <summary>Purchase cost per unit.</summary>
    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    /// <summary>The individual units this line brought in. Empty unless the product is tracked.</summary>
    public List<ProductSerial> Serials { get; set; } = [];
}
