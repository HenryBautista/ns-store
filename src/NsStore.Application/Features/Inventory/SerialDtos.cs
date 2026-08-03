using NsStore.Application.Common.Models;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Inventory;

/// <summary>One tracked unit, as the POS picker and the stock screens see it.</summary>
public record ProductSerialDto(
    long Id,
    long ProductId,
    string ProductName,
    string SerialNumber,
    ProductSerialStatus Status,
    long BranchId,
    string BranchCode,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? SoldAt);

/// <summary>
/// Defaults to what the POS actually asks for: the units this branch can sell right now.
/// </summary>
public record SerialQuery(
    long? ProductId = null,
    long? BranchId = null,
    ProductSerialStatus? Status = ProductSerialStatus.InStock,
    string? Search = null,
    int Page = 1,
    int PageSize = 25)
{
    public PageRequest ToPageRequest() => new(Search, Page, PageSize);
}

/// <summary>
/// Attaches serials to units the branch has already counted, without moving any quantity.
/// </summary>
/// <remarks>
/// The way a shop finishes what switching tracking on started. Without it a product carrying 40
/// untracked units would never become fully identified until all 40 had sold, so the pick rule
/// could never tighten. Deliberately not a stock movement: nothing arrives, the units were always
/// there — they just gain names.
/// </remarks>
public record RegisterSerialsRequest(
    long ProductId,
    IReadOnlyList<string> SerialNumbers,
    long? BranchId = null);

/// <summary>The sale a unit left on, for the warranty desk.</summary>
public record SerialSaleReferenceDto(
    long SaleId,
    string Number,
    DateOnly SaleDate,
    long ClientId,
    string ClientName,
    string? ClientDocument,
    decimal UnitPrice);

/// <summary>
/// The answer to "is this unit ours?". <paramref name="WarrantyTermDescription"/> is free text
/// ("6 MESES", "1 AÑO"), so the expiry is left to the caller to present alongside the sale date
/// rather than computed here from a string the shop can edit.
/// </summary>
public record SerialLookupDto(
    long Id,
    string SerialNumber,
    long ProductId,
    string ProductName,
    string? PartNumber,
    string? WarrantyTermDescription,
    ProductSerialStatus Status,
    long BranchId,
    string BranchCode,
    DateTimeOffset ReceivedAt,
    SerialSaleReferenceDto? Sale);

public record SerialEventDto(
    long Id,
    SerialEventType EventType,
    long BranchId,
    string BranchCode,
    string? ReferenceType,
    long? ReferenceId,
    string? Notes,
    DateTimeOffset CreatedAt);

/// <summary>
/// A (branch, product) pair holding more identified units than stock. Always empty in a healthy
/// system: it is the observability for the one race the locks do not cover, an admin toggling
/// tracking off while a purchase is in flight.
/// </summary>
public record SerialDriftDto(
    long BranchId,
    string BranchCode,
    long ProductId,
    string ProductName,
    int Quantity,
    int SerializedQuantity);
