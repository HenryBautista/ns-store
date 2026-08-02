using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Features.Purchases;
using NsStore.Application.Features.Sales;
using NsStore.Domain.Enums;

namespace NsStore.Api.Endpoints;

public static class PurchaseEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/purchases")
            .WithTags("Purchases")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (
                string? search,
                DateOnly? from,
                DateOnly? to,
                int? page,
                int? pageSize,
                long? branchId,
                PurchaseService purchases,
                CancellationToken ct) =>
            Results.Ok(await purchases.ListAsync(new PurchaseQuery(search, from, to, page ?? 1, pageSize ?? 25, branchId), ct)));

        group.MapGet("/{id:long}", async (long id, PurchaseService purchases, CancellationToken ct) =>
            Results.Ok(await purchases.GetAsync(id, ct)));

        group.MapPost("/", async (CreatePurchaseRequest request, PurchaseService purchases, CancellationToken ct) =>
            {
                var created = await purchases.CreateAsync(request, ct);
                return Results.Created($"/api/v1/purchases/{created.Id}", created);
            })
            .WithValidation<CreatePurchaseRequest>()
            .WithSummary("Register a purchase; increments stock and writes the inventory ledger");

        return app;
    }
}

public static class SaleEndpoints
{
    public static IEndpointRouteBuilder MapSaleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sales")
            .WithTags("Sales")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (
                string? search,
                DateOnly? from,
                DateOnly? to,
                string? status,
                int? page,
                int? pageSize,
                long? branchId,
                long? clientId,
                SaleService sales,
                CancellationToken ct) =>
            Results.Ok(await sales.ListAsync(new SaleQuery(
                search, from, to, QueryEnum.Parse<PaymentStatus>(status, "status"),
                page ?? 1, pageSize ?? 25, branchId, clientId), ct)));

        // Declared before "/{id:long}" so the literal segment wins the route match.
        group.MapGet("/debts", async (string? search, int? page, int? pageSize, long? branchId, long? clientId, SaleService sales, CancellationToken ct) =>
                Results.Ok(await sales.ListDebtsAsync(new SaleQuery(search, null, null, null, page ?? 1, pageSize ?? 25, branchId, clientId), ct)))
            .WithSummary("Credit sales with an outstanding balance");

        // Also before "/{id:long}", and before "/debts" would match it as an id.
        group.MapGet("/debts/by-client", async (
                string? search,
                string? status,
                int? page,
                int? pageSize,
                long? branchId,
                SaleService sales,
                CancellationToken ct) =>
                Results.Ok(await sales.ListDebtsByClientAsync(
                    new ClientDebtQuery(
                        search,
                        QueryEnum.Parse<ClientDebtFilter>(status, "status") ?? ClientDebtFilter.All,
                        page ?? 1, pageSize ?? 25, branchId), ct)))
            .WithSummary("Outstanding balance aggregated per client, worst-overdue first");

        group.MapPost("/collections", async (CollectDebtRequest request, SaleService sales, CancellationToken ct) =>
                Results.Ok(await sales.CollectFromClientAsync(request, ct)))
            .WithValidation<CollectDebtRequest>()
            .WithSummary("Collect one amount from a client, spread oldest-first, and issue a receipt");

        group.MapGet("/collections/{receiptId:long}", async (long receiptId, SaleService sales, CancellationToken ct) =>
                Results.Ok(await sales.GetCollectionReceiptAsync(receiptId, ct)))
            .WithSummary("Reissue a collection receipt");

        group.MapGet("/{id:long}", async (long id, SaleService sales, CancellationToken ct) =>
            Results.Ok(await sales.GetAsync(id, ct)));

        group.MapGet("/{id:long}/items", async (long id, SaleService sales, CancellationToken ct) =>
            Results.Ok(await sales.GetItemsAsync(id, ct)));

        group.MapPost("/", async (CreateSaleRequest request, SaleService sales, CancellationToken ct) =>
            {
                var created = await sales.CreateAsync(request, ct);
                return Results.Created($"/api/v1/sales/{created.Id}", created);
            })
            .WithValidation<CreateSaleRequest>()
            .WithSummary("Register a sale; validates and decrements stock inside one transaction");

        group.MapPost("/{id:long}/payments", async (long id, RegisterPaymentRequest request, SaleService sales, CancellationToken ct) =>
                Results.Ok(await sales.RegisterPaymentAsync(id, request, ct)))
            .WithValidation<RegisterPaymentRequest>()
            .WithSummary("Register an installment against a credit sale");

        return app;
    }
}
