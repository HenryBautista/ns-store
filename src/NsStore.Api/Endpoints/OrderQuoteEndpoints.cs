using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Features.Orders;
using NsStore.Application.Features.Quotes;

namespace NsStore.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders")
            .WithTags("Orders")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, DateOnly? date, int? page, int? pageSize, OrderService orders, CancellationToken ct) =>
            Results.Ok(await orders.ListAsync(new OrderQuery(search, date, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, OrderService orders, CancellationToken ct) =>
            Results.Ok(await orders.GetAsync(id, ct)));

        group.MapPost("/", async (OrderRequest request, OrderService orders, CancellationToken ct) =>
            {
                var created = await orders.CreateAsync(request, ct);
                return Results.Created($"/api/v1/orders/{created.Id}", created);
            })
            .WithValidation<OrderRequest>();

        // Ownership (seller edits only their own) is enforced in the service, not in the UI.
        group.MapPut("/{id:long}", async (long id, OrderRequest request, OrderService orders, CancellationToken ct) =>
                Results.Ok(await orders.UpdateAsync(id, request, ct)))
            .WithValidation<OrderRequest>();

        group.MapDelete("/{id:long}", async (long id, OrderService orders, CancellationToken ct) =>
            {
                await orders.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthPolicies.AdminOnly);

        return app;
    }
}

public static class QuoteEndpoints
{
    public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/quotes")
            .WithTags("Quotes")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, DateOnly? date, int? page, int? pageSize, QuoteService quotes, CancellationToken ct) =>
            Results.Ok(await quotes.ListAsync(new QuoteQuery(search, date, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, QuoteService quotes, CancellationToken ct) =>
            Results.Ok(await quotes.GetAsync(id, ct)));

        group.MapPost("/", async (QuoteRequest request, QuoteService quotes, CancellationToken ct) =>
            {
                var created = await quotes.CreateAsync(request, ct);
                return Results.Created($"/api/v1/quotes/{created.Id}", created);
            })
            .WithValidation<QuoteRequest>();

        group.MapPut("/{id:long}", async (long id, QuoteRequest request, QuoteService quotes, CancellationToken ct) =>
                Results.Ok(await quotes.UpdateAsync(id, request, ct)))
            .WithValidation<QuoteRequest>();

        group.MapDelete("/{id:long}", async (long id, QuoteService quotes, CancellationToken ct) =>
            {
                await quotes.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthPolicies.AdminOnly);

        return app;
    }
}
