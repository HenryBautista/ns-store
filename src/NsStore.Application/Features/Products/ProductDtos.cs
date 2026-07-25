namespace NsStore.Application.Features.Products;

public record ProductDto(
    long Id,
    string Name,
    string? PartNumber,
    string? Description,
    string? SerialNumber,
    long? TrademarkId,
    string? TrademarkName,
    long? CategoryId,
    string? CategoryName,
    long? WarrantyTermId,
    string? WarrantyTermDescription,
    decimal PriceWithInvoice,
    decimal PriceWithoutInvoice,
    int AvailableQuantity);

public record ProductRequest(
    string Name,
    string? PartNumber,
    string? Description,
    string? SerialNumber,
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
