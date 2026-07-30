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

        // branchId is a read hint only: any authenticated caller may ask about any branch's stock.
        group.MapGet("/", async (string? search, int? page, int? pageSize, long? branchId, ProductService products, CancellationToken ct) =>
            Results.Ok(await products.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), branchId, ct)));

        group.MapGet("/{id:long}", async (long id, long? branchId, ProductService products, CancellationToken ct) =>
            Results.Ok(await products.GetAsync(id, branchId, ct)));

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
                long? branchId,
                InventoryService inventory,
                CancellationToken ct) =>
            Results.Ok(await inventory.ListMovementsAsync(id, new PageRequest(null, page ?? 1, pageSize ?? 25), branchId, ct)));

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

        stock.MapGet("/", async (string? search, int? page, int? pageSize, long? branchId, InventoryService inventory, CancellationToken ct) =>
            Results.Ok(await inventory.ListStockAsync(new StockQuery(search, branchId, page ?? 1, pageSize ?? 25), ct)));

        // No branch guard by design: this is the read the whole feature exists for.
        stock.MapGet("/availability", async (long productId, InventoryService inventory, CancellationToken ct) =>
                Results.Ok(await inventory.GetAvailabilityAsync(productId, ct)))
            .WithSummary("Where a product sits across every active branch");

        stock.MapPost("/adjustments", async (StockAdjustmentRequest request, InventoryService inventory, CancellationToken ct) =>
                Results.Ok(await inventory.AdjustAsync(request, ct)))
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithValidation<StockAdjustmentRequest>();

        stock.MapGet("/transfers", async (
                DateOnly? from,
                DateOnly? to,
                long? branchId,
                int? page,
                int? pageSize,
                TransferService transfers,
                CancellationToken ct) =>
            Results.Ok(await transfers.ListAsync(new TransferQuery(from, to, branchId, page ?? 1, pageSize ?? 25), ct)));

        stock.MapGet("/transfers/{id:long}", async (long id, TransferService transfers, CancellationToken ct) =>
            Results.Ok(await transfers.GetAsync(id, ct)));

        stock.MapPost("/transfers", async (CreateTransferRequest request, TransferService transfers, CancellationToken ct) =>
            {
                var created = await transfers.CreateAsync(request, ct);
                return Results.Created($"/api/v1/stock/transfers/{created.Id}", created);
            })
            .WithValidation<CreateTransferRequest>()
            .WithSummary("Move stock between branches. Immutable like a sale — correct a mistake with a reverse transfer, there is no PUT or DELETE");

        app.MapGet("/kardex", async (string? search, int? page, int? pageSize, long? branchId, InventoryService inventory, CancellationToken ct) =>
                Results.Ok(await inventory.GetKardexAsync(new KardexQuery(search, branchId, page ?? 1, pageSize ?? 25), ct)))
            .WithTags("Inventory")
            .RequireAuthorization(AuthPolicies.Authenticated);

        return app;
    }
}
