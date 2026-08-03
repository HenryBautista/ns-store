using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Purchases;

/// <summary>
/// <paramref name="SerialNumbers"/> is mandatory and exact for a tracked product — goods arriving
/// today have no excuse for a missing serial, unlike the stock that was already on the shelf when
/// tracking was switched on. Optional in the signature only so existing positional callers compile.
/// </summary>
public record PurchaseItemRequest(
    long ProductId,
    int Quantity,
    decimal UnitPrice,
    IReadOnlyList<string>? SerialNumbers = null);

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
    decimal Subtotal,
    IReadOnlyList<string> SerialNumbers);

public record PurchaseDto(
    long Id,
    DateOnly PurchaseDate,
    long BranchId,
    string BranchCode,
    string Number,
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
    long BranchId,
    string BranchCode,
    string Number,
    long SupplierId,
    string SupplierName,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    int LineCount,
    int TotalQuantity,
    decimal TotalAmount,
    string? CreatedByName);

/// <summary>See <c>SaleQuery</c> for why <paramref name="BranchId"/> is last and nullable.</summary>
public record PurchaseQuery(
    string? Search,
    DateOnly? From,
    DateOnly? To,
    int Page = 1,
    int PageSize = 25,
    long? BranchId = null);
