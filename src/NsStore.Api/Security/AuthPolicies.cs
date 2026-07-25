namespace NsStore.Api.Security;

public static class AuthPolicies
{
    /// <summary>Any authenticated user (admin or seller).</summary>
    public const string Authenticated = "authenticated";

    /// <summary>Users, settings, price changes and manual stock adjustments.</summary>
    public const string AdminOnly = "admin";

    public const string AdminRole = "admin";
}

public static class AuthCookies
{
    public const string RefreshToken = "ns_refresh_token";

    /// <summary>The cookie is only ever sent to the auth endpoints that need it.</summary>
    public const string RefreshTokenPath = "/api/v1/auth";
}
