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

public record StockAdjustmentRequest(long ProductId, int QuantityDelta, string? Notes);

/// <summary>
/// Per-product ledger summary for one branch. <paramref name="TotalAdjusted"/> is the signed sum of
/// manual adjustments, so <c>Available = TotalPurchased − TotalSold + TotalAdjusted</c> holds
/// without the client inferring it. <paramref name="TotalSoldAmount"/> is the money actually
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
    decimal TotalSoldAmount,
    int Available);
