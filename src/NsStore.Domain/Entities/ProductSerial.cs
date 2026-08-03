using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

/// <summary>
/// One physical unit of a serialized product, tracked from the moment it enters stock until it is
/// sold or written off. The warranty desk answers "did we sell this unit?" by looking a serial up
/// here, so the row is evidence, not master data.
/// </summary>
/// <remarks>
/// Deliberately <b>not</b> an <see cref="AuditableEntity"/>: it carries no soft delete. Were it
/// soft-deletable the global unique index would have to be either unconditional — in which case a
/// deleted serial holds its number forever anyway — or filtered on <c>deleted_at</c>, which would
/// let anyone erase a sold unit's trail and re-register its number under a different sale. That is
/// precisely the fraud this entity exists to prevent. A serial is a physical fact, like
/// <see cref="InventoryMovement"/> and <see cref="SaleItem"/>, neither of which soft-deletes either.
/// A mistyped serial is corrected in place while it is still <see cref="ProductSerialStatus.InStock"/>;
/// a scrapped unit is a negative adjustment that leaves it <see cref="ProductSerialStatus.Removed"/>.
/// </remarks>
public class ProductSerial : IHasCreationAudit
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Globally unique across every product, compared case-insensitively. Stored trimmed but with
    /// the original casing: the printed serial has to match the sticker on the box.
    /// </summary>
    public string SerialNumber { get; set; } = null!;

    public ProductSerialStatus Status { get; set; }

    /// <summary>Where the unit is now — or, once sold or removed, the branch it left from.</summary>
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    /// <summary>Provenance. Null when the unit was registered by adjustment or by back-fill.</summary>
    public long? PurchaseItemId { get; set; }
    public PurchaseItem? PurchaseItem { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    /// <summary>Disposition. Non-null exactly when <see cref="Status"/> is Sold.</summary>
    public long? SaleItemId { get; set; }
    public SaleItem? SaleItem { get; set; }

    public DateTimeOffset? SoldAt { get; set; }

    public List<ProductSerialEvent> Events { get; set; } = [];

    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic token. The pessimistic stock lock is the primary defence against two tills
    /// picking the same serial; this catches whatever slips past it.
    /// </summary>
    public int Version { get; set; }

    /// <summary>Sells the unit, binding it to the line it left on.</summary>
    public void MarkSold(SaleItem saleItem, DateTimeOffset now)
    {
        EnsureInStock("sold");

        Status = ProductSerialStatus.Sold;
        SaleItem = saleItem;
        SoldAt = now;
        Touch(now);
    }

    /// <summary>
    /// Moves the unit to another branch. It stays in stock — a transfer changes where the unit is,
    /// not whether it is available.
    /// </summary>
    public void MarkTransferred(long destinationBranchId, DateTimeOffset now)
    {
        EnsureInStock("transferred");

        if (destinationBranchId == BranchId)
        {
            throw new DomainRuleException(
                ErrorCodes.SerialNotAvailable,
                $"Serial {SerialNumber} is already in branch {BranchId}");
        }

        BranchId = destinationBranchId;
        Touch(now);
    }

    /// <summary>Writes the unit off: scrapped, lost, or corrected away by an adjustment.</summary>
    public void MarkRemoved(DateTimeOffset now)
    {
        EnsureInStock("removed");

        Status = ProductSerialStatus.Removed;
        Touch(now);
    }

    private void EnsureInStock(string action)
    {
        if (Status != ProductSerialStatus.InStock)
        {
            throw new DomainRuleException(
                ErrorCodes.SerialNotAvailable,
                $"Serial {SerialNumber} is {Status} and cannot be {action}");
        }
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }
}

/// <summary>
/// Where a unit has been. <see cref="ProductSerial"/> only knows where the unit is <i>now</i>, so
/// without this a transfer note could never list the serials it moved.
/// </summary>
public class ProductSerialEvent : IHasCreationAudit
{
    public long Id { get; set; }

    public long SerialId { get; set; }
    public ProductSerial Serial { get; set; } = null!;

    public SerialEventType EventType { get; set; }

    /// <summary>The branch the event happened at — for a transfer, one row per side.</summary>
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    /// <summary>One of <c>"sale"</c>, <c>"purchase"</c>, <c>"manual"</c> or <c>"transfer"</c>.</summary>
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public string? Notes { get; set; }

    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
