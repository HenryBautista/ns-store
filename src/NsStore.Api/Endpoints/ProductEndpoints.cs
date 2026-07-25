using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Inventory;
using NsStore.Application.Features.Products;
using NsStore.Application.Features.Sales;

namespace NsStore.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products")
            .WithTags("Products")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, ProductService products, CancellationToken ct) =>
            Results.Ok(await products.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, ProductService products, CancellationToken ct) =>
            Results.Ok(await products.GetAsync(id, ct)));

        group.MapPost("/", async (ProductRequest request, ProductService products, CancellationToken ct) =>
            {
                var created = await products.CreateAsync(request, ct);
                return Results.Created($"/api/v1/products/{created.Id}", created);
            })
            .WithValidation<ProductRequest>();

        group.MapPut("/{id:long}", async (long id, ProductRequest request, ProductService products, CancellationToken ct) =>
                Results.Ok(await products.UpdateAsync(id, request, ct)))
            .WithValidation<ProductRequest>();

        group.MapDelete("/{id:long}", async (long id, ProductService products, CancellationToken ct) =>
        {
            await products.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/{id:long}/price-suggestion", async (long id, ProductService products, CancellationToken ct) =>
            Results.Ok(await products.GetPriceSuggestionAsync(id, ct)));

        // Setting sale prices is admin-only — a tightening over the legacy (see API design doc).
        group.MapPut("/{id:long}/prices", async (long id, SetPricesRequest request, ProductService products, CancellationToken ct) =>
                Results.Ok(await products.SetPricesAsync(id, request, ct)))
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithValidation<SetPricesRequest>();

        group.MapGet("/{id:long}/movements", async (
                long id,
                int? page,
                int? pageSize,
                InventoryService inventory,
                CancellationToken ct) =>
            Results.Ok(await inventory.ListMovementsAsync(id, new PageRequest(null, page ?? 1, pageSize ?? 25), ct)));

        return app;
    }
}

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var stock = app.MapGroup("/stock")
            .WithTags("Inventory")
            .RequireAuthorization(AuthPolicies.Authenticated);

        stock.MapGet("/", async (string? search, int? page, int? pageSize, InventoryService inventory, CancellationToken ct) =>
            Results.Ok(await inventory.ListStockAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        stock.MapPost("/adjustments", async (StockAdjustmentRequest request, InventoryService inventory, CancellationToken ct) =>
                Results.Ok(await inventory.AdjustAsync(request, ct)))
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithValidation<StockAdjustmentRequest>();

        app.MapGet("/kardex", async (string? search, int? page, int? pageSize, InventoryService inventory, CancellationToken ct) =>
                Results.Ok(await inventory.GetKardexAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)))
            .WithTags("Inventory")
            .RequireAuthorization(AuthPolicies.Authenticated);

        return app;
    }
}
