using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Orders;

public record OrderDto(
    long Id,
    DateOnly OrderDate,
    string ClientName,
    string? Phone,
    string ProductDescription,
    decimal Price,
    decimal AdvanceAmount,
    decimal Balance,
    string? Notes,
    OrderStatus Status,
    long OwnerId,
    string OwnerUsername);

public record OrderRequest(
    DateOnly OrderDate,
    string ClientName,
    string? Phone,
    string ProductDescription,
    decimal Price,
    decimal AdvanceAmount,
    string? Notes,
    OrderStatus? Status);

public record OrderQuery(string? Search, DateOnly? Date, int Page = 1, int PageSize = 25);
