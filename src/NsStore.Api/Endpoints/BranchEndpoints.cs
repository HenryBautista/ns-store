using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Branches;

namespace NsStore.Api.Endpoints;

public static class BranchEndpoints
{
    public static IEndpointRouteBuilder MapBranchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/branches")
            .WithTags("Branches")
            // Readable by everyone: a seller needs branch names to read cross-branch availability,
            // and an admin needs the list to populate the branch switcher.
            .RequireAuthorization(AuthPolicies.Authenticated);

        group.MapGet("/", async (string? search, int? page, int? pageSize, BranchService branches, CancellationToken ct) =>
            Results.Ok(await branches.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, BranchService branches, CancellationToken ct) =>
            Results.Ok(await branches.GetAsync(id, ct)));

        group.MapPost("/", async (BranchRequest request, BranchService branches, CancellationToken ct) =>
            {
                var created = await branches.CreateAsync(request, ct);
                return Results.Created($"/api/v1/branches/{created.Id}", created);
            })
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithValidation<BranchRequest>()
            .WithSummary("Create a branch; also creates a zero stock row for every live product");

        group.MapPut("/{id:long}", async (long id, BranchRequest request, BranchService branches, CancellationToken ct) =>
                Results.Ok(await branches.UpdateAsync(id, request, ct)))
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithValidation<BranchRequest>();

        group.MapPut("/{id:long}/status", async (long id, UpdateBranchStatusRequest request, BranchService branches, CancellationToken ct) =>
                Results.Ok(await branches.SetStatusAsync(id, request.IsActive, ct)))
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithSummary("Deactivate a branch; preferred over deleting it");

        group.MapDelete("/{id:long}", async (long id, BranchService branches, CancellationToken ct) =>
            {
                await branches.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .RequireAuthorization(AuthPolicies.AdminOnly)
            .WithSummary("Soft-delete a branch; rejected while it holds stock, users, sales or purchases");

        return app;
    }
}
