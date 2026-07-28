using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Common.Interfaces;

/// <summary>The authenticated caller, resolved from the JWT by the API layer.</summary>
public interface ICurrentUser
{
    long? UserId { get; }
    string? Username { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }

    /// <summary>The caller's home branch, read from the <c>branch</c> claim.</summary>
    long? HomeBranchId { get; }

    /// <summary>
    /// The branch this request operates on: the home branch, or the <c>X-Branch-Id</c> override.
    /// Only an admin may override; a mismatched header is rejected rather than ignored, so a stale
    /// SPA cannot silently write into the wrong branch.
    /// </summary>
    long? ActiveBranchId { get; }
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public record AccessToken(string Value, DateTimeOffset ExpiresAt);

public record IssuedRefreshToken(string RawValue, string Hash, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    IssuedRefreshToken CreateRefreshToken();
    string HashRefreshToken(string rawToken);
}

/// <summary>Identifies one <c>stock_levels</c> row — the unit of locking.</summary>
public readonly record struct StockKey(long BranchId, long ProductId);

/// <summary>
/// Pessimistic lock on stock rows (<c>SELECT ... FOR UPDATE</c>) so concurrent sales of the
/// same product serialize instead of overselling. No-op on providers without row locking.
/// </summary>
/// <remarks>
/// Takes pairs rather than a branch plus product ids on purpose: a two-branch operation such as a
/// transfer would otherwise need two calls, making the order between them a caller decision — and
/// that is exactly what deadlocks an A→B transfer against a concurrent B→A. One pair-based
/// signature means one ordering rule, applied in one place.
/// </remarks>
public interface IStockLockService
{
    Task LockAsync(IReadOnlyCollection<StockKey> keys, CancellationToken cancellationToken = default);
}

public enum DocumentKind { Sale, Purchase, Transfer }

/// <summary>
/// Hands out the next per-branch document number.
/// </summary>
/// <remarks>
/// <para>Backed by a counter column on <c>branches</c>, not a Postgres sequence. Sequences are not
/// transactional: a sale rolled back by <c>INSUFFICIENT_STOCK</c> would burn its number forever, and
/// gaps in a series of fiscal documents are the last thing anyone wants. The counter increments
/// inside the same transaction as the insert, so both revert together.</para>
/// <para><b>Never cache the number outside the action.</b> <c>ExecuteInTransactionAsync</c> retries
/// the whole action under the Npgsql execution strategy, and a retry must read a fresh number.</para>
/// <para>Lock ordering, system-wide: <c>stock_levels</c> first (by branch, product), the branch
/// counter row last. Taking it last minimises how long a branch's sales serialise behind one row.</para>
/// </remarks>
public interface IDocumentNumberService
{
    Task<long> NextAsync(long branchId, DocumentKind kind, CancellationToken cancellationToken = default);
}
