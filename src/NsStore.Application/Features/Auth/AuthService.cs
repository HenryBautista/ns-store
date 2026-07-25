using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Features.Users;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Auth;

public class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);

        // Same response for unknown user, wrong password and disabled account: no user enumeration.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash) || !user.IsActive)
        {
            throw new UnauthorizedException(ErrorCodes.InvalidCredentials, "Invalid username or password");
        }

        return await IssueTokensAsync(user, familyId: Guid.NewGuid(), cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string? rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            throw new UnauthorizedException(ErrorCodes.InvalidRefreshToken, "Missing refresh token");
        }

        var hash = tokenService.HashRefreshToken(rawRefreshToken);
        var stored = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            throw new UnauthorizedException(ErrorCodes.InvalidRefreshToken, "Invalid refresh token");
        }

        var now = clock.GetUtcNow();

        // Reuse of an already-rotated token means the cookie leaked: burn the whole family.
        if (stored.RevokedAt is not null)
        {
            await RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(ErrorCodes.InvalidRefreshToken, "Refresh token reuse detected");
        }

        if (stored.ExpiresAt <= now || !stored.User.IsActive)
        {
            throw new UnauthorizedException(ErrorCodes.InvalidRefreshToken, "Refresh token is no longer valid");
        }

        stored.RevokedAt = now;
        return await IssueTokensAsync(stored.User, stored.FamilyId, cancellationToken);
    }

    public async Task LogoutAsync(string? rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            return;
        }

        var hash = tokenService.HashRefreshToken(rawRefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (stored is null)
        {
            return;
        }

        await RevokeFamilyAsync(stored.FamilyId, clock.GetUtcNow(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException(ErrorCodes.Unauthorized, "No authenticated user");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException(ErrorCodes.Unauthorized, "No authenticated user");

        return user.ToDto();
    }

    private async Task<AuthResult> IssueTokensAsync(User user, Guid familyId, CancellationToken cancellationToken)
    {
        var access = tokenService.CreateAccessToken(user);
        var refresh = tokenService.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.Hash,
            FamilyId = familyId,
            ExpiresAt = refresh.ExpiresAt,
            CreatedAt = clock.GetUtcNow()
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(access.Value, access.ExpiresAt, user.ToDto(), refresh.RawValue, refresh.ExpiresAt);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var family = await db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in family)
        {
            token.RevokedAt = now;
        }
    }
}
