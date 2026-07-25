using NsStore.Api.Middleware;
using NsStore.Api.Security;
using NsStore.Application.Features.Auth;

namespace NsStore.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest request, AuthService auth, HttpContext http, CancellationToken ct) =>
            {
                var result = await auth.LoginAsync(request, ct);
                SetRefreshCookie(http, result);
                return Results.Ok(new LoginResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User));
            })
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Login)
            .WithValidation<LoginRequest>()
            .WithSummary("Sign in and receive an access token plus a refresh cookie");

        group.MapPost("/refresh", async (AuthService auth, HttpContext http, CancellationToken ct) =>
            {
                var result = await auth.RefreshAsync(http.Request.Cookies[AuthCookies.RefreshToken], ct);
                SetRefreshCookie(http, result);
                return Results.Ok(new LoginResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User));
            })
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Login)
            .WithSummary("Rotate the refresh cookie and issue a new access token");

        group.MapPost("/logout", async (AuthService auth, HttpContext http, CancellationToken ct) =>
            {
                await auth.LogoutAsync(http.Request.Cookies[AuthCookies.RefreshToken], ct);
                http.Response.Cookies.Delete(AuthCookies.RefreshToken, new CookieOptions
                {
                    Path = AuthCookies.RefreshTokenPath
                });
                return Results.NoContent();
            })
            .AllowAnonymous()
            .WithSummary("Revoke the refresh token family and clear the cookie");

        group.MapGet("/me", async (AuthService auth, CancellationToken ct) => Results.Ok(await auth.GetCurrentUserAsync(ct)))
            .RequireAuthorization(AuthPolicies.Authenticated)
            .WithSummary("Current user profile and role");

        return app;
    }

    private static void SetRefreshCookie(HttpContext http, AuthResult result)
    {
        http.Response.Cookies.Append(AuthCookies.RefreshToken, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = AuthCookies.RefreshTokenPath,
            Expires = result.RefreshTokenExpiresAt
        });
    }
}
