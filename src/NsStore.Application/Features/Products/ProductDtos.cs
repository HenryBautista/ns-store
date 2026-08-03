namespace NsStore.Application.Features.Products;

/// <summary>
/// <paramref name="AvailableQuantity"/> is the active branch's holding; <paramref name="QuantityAllBranches"/>
/// is the system-wide total. That single extra field is what lets the POS show "in stock elsewhere"
/// straight from the search results, with no second request.
/// <paramref name="SerializedQuantity"/> counts the active branch's units that carry a serial, so the
/// POS can work out how many must be picked without a second call.
/// </summary>
public record ProductDto(
    long Id,
    string Name,
    string? PartNumber,
    string? Description,
    bool IsSerialized,
    long? TrademarkId,
    string? TrademarkName,
    long? CategoryId,
    string? CategoryName,
    long? WarrantyTermId,
    string? WarrantyTermDescription,
    decimal PriceWithInvoice,
    decimal PriceWithoutInvoice,
    int AvailableQuantity,
    int QuantityAllBranches,
    int SerializedQuantity);

public record ProductRequest(
    string Name,
    string? PartNumber,
    string? Description,
    bool IsSerialized,
    long? TrademarkId,
    long? CategoryId,
    long? WarrantyTermId);

public record SetPricesRequest(decimal PriceWithInvoice, decimal PriceWithoutInvoice);

/// <summary>
/// Suggestion derived from the latest purchase cost and the configured margin/VAT.
/// <paramref name="LastCost"/> is null when the product has no purchase history.
/// </summary>
public record PriceSuggestionDto(
    long ProductId,
    decimal? LastCost,
    decimal MarginPct,
    decimal VatPct,
    decimal? SuggestedWithoutInvoice,
    decimal? SuggestedWithInvoice,
    decimal CurrentPriceWithInvoice,
    decimal CurrentPriceWithoutInvoice);
