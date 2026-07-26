using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Purchases;

public record PurchaseItemRequest(long ProductId, int Quantity, decimal UnitPrice);

public record CreatePurchaseRequest(
    DateOnly PurchaseDate,
    long SupplierId,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    IReadOnlyList<PurchaseItemRequest> Items);

public record PurchaseItemDto(
    long Id,
    long ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record PurchaseDto(
    long Id,
    DateOnly PurchaseDate,
    long SupplierId,
    string SupplierName,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    int TotalQuantity,
    decimal TotalAmount,
    long? CreatedBy,
    string? CreatedByName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PurchaseItemDto> Items);

/// <summary><paramref name="LineCount"/> is the number of distinct products on the purchase.</summary>
public record PurchaseListItemDto(
    long Id,
    DateOnly PurchaseDate,
    long SupplierId,
    string SupplierName,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    int LineCount,
    int TotalQuantity,
    decimal TotalAmount,
    string? CreatedByName);

public record PurchaseQuery(string? Search, DateOnly? From, DateOnly? To, int Page = 1, int PageSize = 25);
