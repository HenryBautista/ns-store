using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

public record StockLevelDto(long ProductId, string ProductName, string? PartNumber, int Quantity, DateTimeOffset UpdatedAt);

public record InventoryMovementDto(
    long Id,
    long ProductId,
    string ProductName,
    MovementType MovementType,
    int QuantityDelta,
    decimal? UnitCost,
    string? ReferenceType,
    long? ReferenceId,
    string? Notes,
    DateTimeOffset CreatedAt);

public record StockAdjustmentRequest(long ProductId, int QuantityDelta, string? Notes);

public record KardexRowDto(long ProductId, string Name, int TotalPurchased, int TotalSold, int Available);
