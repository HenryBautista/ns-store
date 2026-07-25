using NsStore.Domain.Common;
using NsStore.Domain.Enums;

namespace NsStore.Domain.Entities;

/// <summary>Customer request ("encargo") for an item that may not be in the catalog, with an advance.</summary>
public class Order : AuditableEntity
{
    public DateOnly OrderDate { get; set; }

    /// <summary>Free text — the requester is not necessarily a catalog client.</summary>
    public string ClientName { get; set; } = null!;

    public string? Phone { get; set; }
    public string ProductDescription { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal AdvanceAmount { get; set; }
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>Owner drives the edit permission: a seller may edit only their own records.</summary>
    public long OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    public decimal Balance => Price - AdvanceAmount;

    public void EnsureAdvanceWithinPrice()
    {
        if (AdvanceAmount > Price)
        {
            throw new DomainRuleException(
                ErrorCodes.AdvanceExceedsPrice,
                $"Advance {AdvanceAmount} exceeds price {Price}");
        }
    }
}

/// <summary>Quotation / proforma ("cotización").</summary>
public class Quote : AuditableEntity
{
    public DateOnly QuoteDate { get; set; }
    public string ClientName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Detail { get; set; } = null!;
    public decimal Price { get; set; }

    /// <summary>Free text, as in the legacy system (not a catalog supplier reference).</summary>
    public string? SupplierName { get; set; }

    public long OwnerId { get; set; }
    public User Owner { get; set; } = null!;
}
