using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Common.Models;
using NsStore.Application.Features.Users;

namespace NsStore.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(AuthPolicies.AdminOnly);

        group.MapGet("/", async (string? search, int? page, int? pageSize, UserService users, CancellationToken ct) =>
            Results.Ok(await users.ListAsync(new PageRequest(search, page ?? 1, pageSize ?? 25), ct)));

        group.MapGet("/{id:long}", async (long id, UserService users, CancellationToken ct) =>
            Results.Ok(await users.GetAsync(id, ct)));

        group.MapPost("/", async (CreateUserRequest request, UserService users, CancellationToken ct) =>
            {
                var created = await users.CreateAsync(request, ct);
                return Results.Created($"/api/v1/users/{created.Id}", created);
            })
            .WithValidation<CreateUserRequest>();

        group.MapPut("/{id:long}", async (long id, UpdateUserRequest request, UserService users, CancellationToken ct) =>
                Results.Ok(await users.UpdateAsync(id, request, ct)))
            .WithValidation<UpdateUserRequest>();

        group.MapPatch("/{id:long}/status", async (long id, UpdateUserStatusRequest request, UserService users, CancellationToken ct) =>
            Results.Ok(await users.SetStatusAsync(id, request.IsActive, ct)));

        group.MapPatch("/{id:long}/role", async (long id, UpdateUserRoleRequest request, UserService users, CancellationToken ct) =>
            Results.Ok(await users.SetRoleAsync(id, request.Role, ct)));

        return app;
    }
}
