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

/// <summary>
/// Pessimistic lock on stock rows (<c>SELECT ... FOR UPDATE</c>) so concurrent sales of the
/// same product serialize instead of overselling. No-op on providers without row locking.
/// </summary>
public interface IStockLockService
{
    Task LockAsync(IReadOnlyCollection<long> productIds, CancellationToken cancellationToken = default);
}
