using NsStore.Application.Common.Models;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

/// <summary>
/// <paramref name="LastCost"/> is the unit cost of the product's most recent purchase, and
/// <paramref name="InventoryValue"/> is <c>Quantity × LastCost</c> — the valuation rule. Both are
/// null/zero until the product has purchase history. They travel with the row so listing stock
/// does not need one price-suggestion call per product.
/// </summary>
public record StockLevelDto(
    long ProductId,
    string ProductName,
    string? PartNumber,
    string? TrademarkName,
    string? CategoryName,
    long BranchId,
    string BranchCode,
    int Quantity,
    decimal? LastCost,
    decimal InventoryValue,
    DateTimeOffset UpdatedAt);

public record InventoryMovementDto(
    long Id,
    long ProductId,
    string ProductName,
    long BranchId,
    string BranchCode,
    MovementType MovementType,
    int QuantityDelta,
    decimal? UnitCost,
    string? ReferenceType,
    long? ReferenceId,
    string? Notes,
    DateTimeOffset CreatedAt);

/// <summary><paramref name="BranchId"/> defaults to the caller's active branch; an admin may target another.</summary>
public record StockAdjustmentRequest(long ProductId, int QuantityDelta, string? Notes, long? BranchId = null);

/// <summary>
/// One branch's holding of a product. Returned for every active branch, to every authenticated
/// caller, with no branch guard — a seller seeing that three units sit in another store is the
/// use case this whole feature exists for.
/// </summary>
public record BranchAvailabilityDto(
    long BranchId,
    string BranchCode,
    string BranchName,
    int Quantity,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Local to the feature rather than fields on <c>PageRequest</c>: that record is shared with
/// clients, catalogs, users, orders and quotes, where a branch means nothing.
/// </summary>
public record StockQuery(string? Search, long? BranchId = null, int Page = 1, int PageSize = 25)
{
    public PageRequest ToPageRequest() => new(Search, Page, PageSize);
}

public record KardexQuery(string? Search, long? BranchId = null, int Page = 1, int PageSize = 25)
{
    public PageRequest ToPageRequest() => new(Search, Page, PageSize);
}

/// <summary>
/// Per-product ledger summary for one branch. The identity the client can rely on is
/// <c>Available = TotalPurchased − TotalSold + TotalAdjusted + TotalTransferredIn − TotalTransferredOut</c>.
/// Transfers contribute two separate figures because dispatching and receiving are distinct
/// physical events at distinct counters. <paramref name="TotalSoldAmount"/> is the money actually
/// invoiced for the product (sum of sale line subtotals), not units revalued at today's price.
/// </summary>
public record KardexRowDto(
    long ProductId,
    string Name,
    string? PartNumber,
    string? TrademarkName,
    long BranchId,
    int TotalPurchased,
    int TotalSold,
    int TotalAdjusted,
    int TotalTransferredIn,
    int TotalTransferredOut,
    decimal TotalSoldAmount,
    int Available);
