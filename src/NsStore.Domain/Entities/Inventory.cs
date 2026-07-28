using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

/// <summary>
/// Current quantity cache: exactly one row per (branch, product), never deleted (may sit at 0).
/// The ledger (<see cref="InventoryMovement"/>) is the source of truth.
/// </summary>
/// <remarks>
/// The grid must stay dense — every (active branch × live product) pair owns a row — because
/// <c>SELECT … FOR UPDATE</c> only locks rows that exist. A missing row turns the pessimistic
/// lock into a no-op and oversell becomes possible again.
/// </remarks>
public class StockLevel
{
    public long BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Optimistic concurrency token; guards against oversell under concurrent sales.</summary>
    public int Version { get; set; }

    public void Apply(int quantityDelta, DateTimeOffset now)
    {
        var result = Quantity + quantityDelta;
        if (result < 0)
        {
            throw new DomainRuleException(
                ErrorCodes.InsufficientStock,
                $"Product {ProductId} in branch {BranchId} requested {-quantityDelta}, available {Quantity}");
        }

        Quantity = result;
        UpdatedAt = now;
        Version++;
    }
}

/// <summary>Immutable inventory ledger entry. Feeds the kardex and the price suggestion.</summary>
public class InventoryMovement : IHasCreationAudit
{
    public long Id { get; set; }

    /// <summary>Which branch's stock moved.</summary>
    public long BranchId { get; set; }

    public Branch Branch { get; set; } = null!;
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public MovementType MovementType { get; set; }

    /// <summary>Positive for inbound, negative for outbound. Never 0.</summary>
    public int QuantityDelta { get; set; }

    /// <summary>Unit cost for purchases; feeds the sale-price suggestion.</summary>
    public decimal? UnitCost { get; set; }

    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
