using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common;
using NsStore.Application.Common.Interfaces;
using NsStore.Application.Common.Models;
using NsStore.Domain.Common;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Application.Features.Users;

public class UserService(IAppDbContext db, IPasswordHasher passwordHasher, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<PagedResult<UserDto>> ListAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Users.AsNoTracking().AsQueryable();
        var search = request.TrimmedSearch;
        if (search is not null)
        {
            var pattern = $"%{search.ToLower()}%";
            query = query.Where(u =>
                EF.Functions.Like(u.Username.ToLower(), pattern) ||
                EF.Functions.Like(u.FirstName.ToLower(), pattern) ||
                EF.Functions.Like(u.LastName.ToLower(), pattern));
        }

        var page = await query
            .OrderBy(u => u.Username)
            .ToPagedResultAsync(request, cancellationToken);

        return new PagedResult<UserDto>(
            page.Items.Select(u => u.ToDto()).ToList(),
            page.Page,
            page.PageSize,
            page.Total);
    }

    public async Task<UserDto> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        await EnsureUsernameAvailableAsync(username, null, cancellationToken);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            MotherLastName = request.MotherLastName?.Trim(),
            Role = request.Role ?? UserRole.Seller,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);
        var username = request.Username.Trim();
        await EnsureUsernameAvailableAsync(username, id, cancellationToken);

        user.Username = username;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.MotherLastName = request.MotherLastName?.Trim();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordHasher.Hash(request.Password);
            // A credential change invalidates existing sessions.
            await RevokeAllTokensAsync(user.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto> SetStatusAsync(long id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);
        if (!isActive && user.Id == currentUser.UserId)
        {
            throw new ConflictException(ErrorCodes.Conflict, "You cannot disable your own account");
        }

        user.IsActive = isActive;
        if (!isActive)
        {
            await RevokeAllTokensAsync(user.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto> SetRoleAsync(long id, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);
        if (user.Id == currentUser.UserId && role != UserRole.Admin)
        {
            throw new ConflictException(ErrorCodes.Conflict, "You cannot remove your own admin role");
        }

        user.Role = role;
        await db.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    private async Task<User> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(User), id);

    private async Task EnsureUsernameAvailableAsync(string username, long? excludeId, CancellationToken cancellationToken)
    {
        var taken = await db.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() && (excludeId == null || u.Id != excludeId), cancellationToken);

        if (taken)
        {
            throw new ConflictException(ErrorCodes.DuplicateUsername, $"Username '{username}' is already taken");
        }
    }

    private async Task RevokeAllTokensAsync(long userId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
        }
    }
}
