using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Clients;
using NsStore.Application.Features.Sales;

namespace NsStore.Api.Endpoints;

public static class ClientEndpoints
{
    public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/clients")
            .WithTags("Clients")
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, ClientService clients, CancellationToken ct) =>
            Results.Ok(await clients.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, ClientService clients, CancellationToken ct) =>
            Results.Ok(await clients.GetAsync(id, ct)));

        group.MapPost("/", async (ClientRequest request, ClientService clients, CancellationToken ct) =>
            {
                var created = await clients.CreateAsync(request, ct);
                return Results.Created($"/api/v1/clients/{created.Id}", created);
            })
            .WithValidation<ClientRequest>();

        group.MapPut("/{id:long}", async (long id, ClientRequest request, ClientService clients, CancellationToken ct) =>
                Results.Ok(await clients.UpdateAsync(id, request, ct)))
            .WithValidation<ClientRequest>();

        group.MapDelete("/{id:long}", async (long id, ClientService clients, CancellationToken ct) =>
        {
            await clients.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/{id:long}/sales", async (long id, int? page, int? pageSize, SaleService sales, CancellationToken ct) =>
            Results.Ok(await sales.ListByClientAsync(id, new PageRequest(null, page ?? 1, pageSize ?? 25), ct)));

        return app;
    }
}
