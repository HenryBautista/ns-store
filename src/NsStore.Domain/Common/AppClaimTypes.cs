namespace NsStore.Domain.Common;

/// <summary>
/// Claim and header names shared by the token issuer (Infrastructure) and the claim reader (Api).
/// Domain is the only assembly both of them reference.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>The user's home branch id. Raw name survives because <c>MapInboundClaims</c> is off.</summary>
    public const string Branch = "branch";

    /// <summary>
    /// Per-request override of the active branch. Only an admin may send a value other than their
    /// home branch; anything else is rejected rather than silently ignored.
    /// </summary>
    public const string BranchHeader = "X-Branch-Id";
}
