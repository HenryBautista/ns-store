namespace NsStore.Api.Middleware;

public static class RateLimitPolicies
{
    /// <summary>Applied to the credential endpoints (`/auth/login`, `/auth/refresh`).</summary>
    public const string Login = "auth-login";
}
