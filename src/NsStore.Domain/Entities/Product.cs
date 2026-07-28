using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

public class Product : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? PartNumber { get; set; }
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }

    public long? TrademarkId { get; set; }
    public Trademark? Trademark { get; set; }
    public long? CategoryId { get; set; }
    public Category? Category { get; set; }
    public long? WarrantyTermId { get; set; }
    public WarrantyTerm? WarrantyTerm { get; set; }

    /// <summary>Sale price used when the sale is issued with an invoice. Set in the pricing module.</summary>
    public decimal PriceWithInvoice { get; set; }

    /// <summary>Sale price used when the sale is issued without an invoice.</summary>
    public decimal PriceWithoutInvoice { get; set; }

    /// <summary>One row per branch. A projection that reads a quantity must say which branch.</summary>
    public List<StockLevel> StockLevels { get; set; } = [];

    public decimal PriceFor(InvoiceType invoiceType) =>
        invoiceType == InvoiceType.WithInvoice ? PriceWithInvoice : PriceWithoutInvoice;
}
