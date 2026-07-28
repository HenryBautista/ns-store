using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

/// <summary>
/// Current quantity cache: exactly one row per product, never deleted (may sit at 0).
/// The ledger (<see cref="InventoryMovement"/>) is the source of truth.
/// </summary>
public class StockLevel
{
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
                $"Product {ProductId} requested {-quantityDelta}, available {Quantity}");
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
