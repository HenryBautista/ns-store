using NsStore.Domain.Common;

namespace NsStore.Domain.Entities;

/// <summary>
/// A physical store. Stock, sales and purchases are scoped to one; the product catalog,
/// the price list and the business settings stay global.
/// </summary>
/// <remarks>
/// Deliberately carries no soft-delete query filter even though it inherits
/// <see cref="AuditableEntity"/> for the audit columns. <c>Sale.Branch</c>, <c>StockLevel.Branch</c>
/// and friends are required navigations, and EF propagates a principal's filter through them: a
/// soft-deleted branch would silently drop its sales out of every report that reads
/// <c>s.Branch.Code</c>. Lifecycle is <see cref="IsActive"/> instead, enforced in the service layer.
/// </remarks>
public class Branch : AuditableEntity
{
    /// <summary>Short uppercase key used as the document number prefix, e.g. <c>MAIN</c>.</summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>Printed on the report letterhead, so it belongs to the branch, not the company.</summary>
    public string? Address { get; set; }

    public string? Phone { get; set; }

    /// <summary>Inactive branches reject new writes; their history stays reachable.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Last document number issued for this branch. See the numbering design (phase 3).</summary>
    public long SaleSequence { get; set; }

    public long PurchaseSequence { get; set; }

    public long TransferSequence { get; set; }

    public long ReceiptSequence { get; set; }

    /// <summary>The single definition of the printed folio format, so it is unit-testable.</summary>
    public string FormatDocumentNumber(long sequence) => $"{Code}-{sequence:D6}";
}
