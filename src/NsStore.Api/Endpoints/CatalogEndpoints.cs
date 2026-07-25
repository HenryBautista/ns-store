using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Catalogs;

namespace NsStore.Api.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        MapTrademarks(app);
        MapCategories(app);
        MapWarrantyTerms(app);
        MapSuppliers(app);
        return app;
    }

    private static void MapTrademarks(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/trademarks")
            .WithTags("Catalogs")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, TrademarkService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, TrademarkService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(id, ct)));

        group.MapPost("/", async (NameRequest request, TrademarkService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/trademarks/{created.Id}", created);
            })
            .WithValidation<NameRequest>();

        group.MapPut("/{id:long}", async (long id, NameRequest request, TrademarkService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<NameRequest>();

        group.MapDelete("/{id:long}", async (long id, TrademarkService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }

    private static void MapCategories(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories")
            .WithTags("Catalogs")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, CategoryService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, CategoryService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(id, ct)));

        group.MapPost("/", async (NameRequest request, CategoryService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/categories/{created.Id}", created);
            })
            .WithValidation<NameRequest>();

        group.MapPut("/{id:long}", async (long id, NameRequest request, CategoryService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<NameRequest>();

        group.MapDelete("/{id:long}", async (long id, CategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }

    private static void MapWarrantyTerms(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/warranty-terms")
            .WithTags("Catalogs")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, WarrantyTermService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, WarrantyTermService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(id, ct)));

        group.MapPost("/", async (DescriptionRequest request, WarrantyTermService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/warranty-terms/{created.Id}", created);
            })
            .WithValidation<DescriptionRequest>();

        group.MapPut("/{id:long}", async (long id, DescriptionRequest request, WarrantyTermService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<DescriptionRequest>();

        group.MapDelete("/{id:long}", async (long id, WarrantyTermService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }

    private static void MapSuppliers(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/suppliers")
            .WithTags("Catalogs")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, SupplierService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, SupplierService service, CancellationToken ct) =>
            Results.Ok(await service.GetAsync(id, ct)));

        group.MapPost("/", async (SupplierRequest request, SupplierService service, CancellationToken ct) =>
            {
                var created = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/suppliers/{created.Id}", created);
            })
            .WithValidation<SupplierRequest>();

        group.MapPut("/{id:long}", async (long id, SupplierRequest request, SupplierService service, CancellationToken ct) =>
                Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithValidation<SupplierRequest>();

        group.MapDelete("/{id:long}", async (long id, SupplierService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
