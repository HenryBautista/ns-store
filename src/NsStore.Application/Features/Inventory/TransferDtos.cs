using NsStore.Application.Common.Models;

namespace NsStore.Application.Features.Inventory;

public record TransferItemRequest(long ProductId, int Quantity);

/// <summary>
/// The one write that names its branches explicitly, because it is intrinsically two-branch.
/// Everything else takes the active branch from the header.
/// </summary>
public record CreateTransferRequest(
    DateOnly TransferDate,
    long OriginBranchId,
    long DestinationBranchId,
    string? Notes,
    IReadOnlyList<TransferItemRequest> Items);

public record TransferItemDto(
    long Id,
    long ProductId,
    string ProductName,
    string? PartNumber,
    int Quantity);

public record TransferDto(
    long Id,
    string Number,
    DateOnly TransferDate,
    long OriginBranchId,
    string OriginBranchCode,
    long DestinationBranchId,
    string DestinationBranchCode,
    int TotalQuantity,
    string? Notes,
    long? CreatedBy,
    string? CreatedByName,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TransferItemDto> Items);

public record TransferListItemDto(
    long Id,
    string Number,
    DateOnly TransferDate,
    long OriginBranchId,
    string OriginBranchCode,
    long DestinationBranchId,
    string DestinationBranchCode,
    int LineCount,
    int TotalQuantity,
    string? CreatedByName);

/// <summary>
/// <paramref name="BranchId"/> matches a transfer on <em>either</em> side: a branch's transfer list
/// should show both what it sent and what it received.
/// </summary>
public record TransferQuery(
    DateOnly? From = null,
    DateOnly? To = null,
    long? BranchId = null,
    int Page = 1,
    int PageSize = 25)
{
    public PageRequest ToPageRequest() => new(null, Page, PageSize);
}
