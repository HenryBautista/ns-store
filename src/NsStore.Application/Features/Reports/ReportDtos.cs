using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Reports;

/// <summary>
/// Reports return structured data only; rendering (print view or PDF) lives in the SPA.
/// </summary>
public record ReportRange(DateOnly? From, DateOnly? To);

public record SalesReportDto(
    DateOnly? From,
    DateOnly? To,
    int SaleCount,
    int TotalQuantity,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalBalance,
    IReadOnlyList<SaleListItemDto> Sales);

public record PurchasesReportDto(
    DateOnly? From,
    DateOnly? To,
    int PurchaseCount,
    int TotalQuantity,
    decimal TotalAmount,
    IReadOnlyList<PurchaseListItemDto> Purchases);

/// <summary><paramref name="TotalValue"/> values the inventory at each product's last purchase cost.</summary>
public record StockReportDto(
    int ProductCount,
    int TotalUnits,
    decimal TotalValue,
    IReadOnlyList<StockLevelDto> Items);

public record DebtsReportDto(int SaleCount, decimal TotalDebt, IReadOnlyList<SaleListItemDto> Sales);

public record PriceListRowDto(
    long ProductId,
    string Name,
    string? PartNumber,
    string? TrademarkName,
    string? CategoryName,
    decimal PriceWithInvoice,
    decimal PriceWithoutInvoice,
    int AvailableQuantity,
    int QuantityAllBranches);

public record PriceListReportDto(string Currency, IReadOnlyList<PriceListRowDto> Items);

/// <summary>Warranty note ("nota de garantía"): standard for a cash sale, credit variant otherwise.</summary>
public record WarrantyNoteDto(string NoteType, SaleDto Sale);

/// <summary><paramref name="BranchId"/> is null when the figures span every branch (admin only).</summary>
public record DashboardDto(
    long? BranchId,
    DateOnly Date,
    decimal SalesTodayAmount,
    int SalesTodayCount,
    decimal SalesMonthAmount,
    int SalesMonthCount,
    decimal PurchasesMonthAmount,
    decimal OutstandingDebt,
    int OutstandingDebtCount,
    int ProductCount,
    int StockUnits,
    int OutOfStockProducts,
    int PendingOrders,
    int QuoteCount);
