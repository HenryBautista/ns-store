using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Sales;

public record SaleItemRequest(long ProductId, int Quantity);

public record CreateSaleRequest(
    DateOnly SaleDate,
    long ClientId,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    decimal? InitialPaid,
    IReadOnlyList<SaleItemRequest> Items);

public record RegisterPaymentRequest(decimal Amount, DateOnly? PaymentDate);

/// <summary>Warranty-note data travels with the line: the printed note lists each product's warranty.</summary>
public record SaleItemDto(
    long Id,
    long ProductId,
    string ProductName,
    string? PartNumber,
    string? SerialNumber,
    string? WarrantyTermDescription,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record PaymentDto(long Id, long SaleId, decimal Amount, DateOnly PaymentDate, DateTimeOffset CreatedAt, string? CreatedByName);

public record SaleDto(
    long Id,
    DateOnly SaleDate,
    long ClientId,
    string ClientName,
    string? ClientNit,
    string? ClientCi,
    string? ClientPhone,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    int TotalQuantity,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal Balance,
    long? CreatedBy,
    string? CreatedByName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<PaymentDto> Payments);

public record SaleListItemDto(
    long Id,
    DateOnly SaleDate,
    long ClientId,
    string ClientName,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    int TotalQuantity,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal Balance,
    string? CreatedByName);

public record SaleQuery(
    string? Search,
    DateOnly? From,
    DateOnly? To,
    PaymentStatus? Status,
    int Page = 1,
    int PageSize = 25);
