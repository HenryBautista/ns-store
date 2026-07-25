using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Catalogs;

public record TrademarkDto(long Id, string Name);

public record CategoryDto(long Id, string Name);

public record WarrantyTermDto(long Id, string Description);

public record SupplierDto(long Id, string Name, string? Phone, string? Email);

public record NameRequest(string Name);

public record DescriptionRequest(string Description);

public record SupplierRequest(string Name, string? Phone, string? Email);

public static class CatalogMapping
{
    public static TrademarkDto ToDto(this Trademark e) => new(e.Id, e.Name);

    public static CategoryDto ToDto(this Category e) => new(e.Id, e.Name);

    public static WarrantyTermDto ToDto(this WarrantyTerm e) => new(e.Id, e.Description);

    public static SupplierDto ToDto(this Supplier e) => new(e.Id, e.Name, e.Phone, e.Email);
}
