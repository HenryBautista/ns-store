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

/// <summary><paramref name="BranchId"/> is the branch that received the money, not the one that sold.</summary>
public record PaymentDto(
    long Id,
    long SaleId,
    long BranchId,
    decimal Amount,
    DateOnly PaymentDate,
    DateTimeOffset CreatedAt,
    string? CreatedByName);

public record SaleDto(
    long Id,
    DateOnly SaleDate,
    long BranchId,
    string BranchCode,
    string Number,
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

/// <summary>
/// <paramref name="DaysOutstanding"/> counts from the last instalment, or from the sale when none
/// has been paid: a client who is paying down an old debt is not treated as if they had vanished.
/// It and <paramref name="IsOverdue"/> are resolved server-side against the configurable
/// <c>overdue_days</c>, so no screen or printed sheet carries its own copy of the rule.
/// </summary>
public record SaleListItemDto(
    long Id,
    DateOnly SaleDate,
    long BranchId,
    string BranchCode,
    string Number,
    long ClientId,
    string ClientName,
    string? ClientDocument,
    InvoiceType InvoiceType,
    PaymentStatus PaymentStatus,
    int TotalQuantity,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal Balance,
    DateOnly? LastPaymentDate,
    int DaysOutstanding,
    bool IsOverdue,
    string? CreatedByName);

/// <summary>
/// <paramref name="BranchId"/> is last with a default so existing positional callers keep compiling.
/// Null means "the caller's scope": their own branch for a seller, all branches for an admin.
/// </summary>
public record SaleQuery(
    string? Search,
    DateOnly? From,
    DateOnly? To,
    PaymentStatus? Status,
    int Page = 1,
    int PageSize = 25,
    long? BranchId = null,
    long? ClientId = null);

/// <summary>Which clients the collections screen wants: everyone owing, or only one side of the due date.</summary>
public enum ClientDebtFilter
{
    All,
    Overdue,
    Current
}

/// <summary>
/// One client's outstanding position, aggregated across every sale they still owe on.
/// <paramref name="DaysOutstanding"/> runs from their last instalment — on any of their sales —
/// falling back to the oldest unpaid sale, so a client paying something is not shown as abandoned.
/// </summary>
public record ClientDebtDto(
    long ClientId,
    string ClientName,
    string? Document,
    string? Phone,
    int SaleCount,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal Balance,
    DateOnly OldestSaleDate,
    DateOnly? LastPaymentDate,
    int DaysOutstanding,
    bool IsOverdue);

public record ClientDebtQuery(
    string? Search,
    ClientDebtFilter Status = ClientDebtFilter.All,
    int Page = 1,
    int PageSize = 25,
    long? BranchId = null);

/// <summary>One sale a collection is being applied to, and how much of it that sale absorbs.</summary>
public record CollectAllocationRequest(long SaleId, decimal Amount);

/// <summary>
/// Collect <paramref name="Amount"/> from a client. With <paramref name="Allocations"/> the caller
/// says which sales absorb it — at the counter one often settles a named invoice rather than handing
/// over a loose sum. Without them the amount still spreads oldest-first, which stays the default so
/// a debt cannot age forever while the client keeps paying.
/// Either way <see cref="Domain.Entities.Sale.RegisterPayment"/> re-checks each balance inside the
/// transaction, so an allocation built from figures a second till has already moved is rejected
/// rather than silently overpaid.
/// </summary>
public record CollectDebtRequest(
    long ClientId,
    decimal Amount,
    DateOnly? PaymentDate,
    IReadOnlyList<CollectAllocationRequest>? Allocations = null);

/// <summary>What one sale absorbed of a collection, and what it still owes afterwards.</summary>
public record PaymentAllocationDto(
    long SaleId,
    string SaleNumber,
    DateOnly SaleDate,
    decimal SaleTotal,
    decimal Applied,
    decimal RemainingBalance,
    bool Settled);

/// <summary>The customer's proof of payment: what they handed over and where it landed.</summary>
public record CollectionReceiptDto(
    long ReceiptId,
    string Number,
    long BranchId,
    string BranchCode,
    long ClientId,
    string ClientName,
    string? ClientDocument,
    string? ClientPhone,
    DateOnly ReceiptDate,
    decimal TotalCollected,
    decimal RemainingDebt,
    string? CreatedByName,
    IReadOnlyList<PaymentAllocationDto> Allocations);
