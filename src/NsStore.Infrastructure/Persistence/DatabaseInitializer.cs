using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Entities;
using NsStore.Domain.Enums;

namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and seeds the minimum a fresh install needs: business parameters and,
/// when credentials are supplied out-of-band, the first admin account.
/// </summary>
public class DatabaseInitializer(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger,
    TimeProvider clock)
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedSettingsAsync(cancellationToken);
        await SeedAdminAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSettingsAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var existing = await db.AppSettings.Select(s => s.Key).ToListAsync(cancellationToken);

        // Legacy constants as starting values — the business can change them from the Settings screen.
        var defaults = new Dictionary<string, string>
        {
            [AppSettingKeys.VatRate] = "16",
            [AppSettingKeys.DefaultMarginPct] = "30",
            [AppSettingKeys.Currency] = "BOB"
        };

        foreach (var (key, value) in defaults.Where(d => !existing.Contains(d.Key)))
        {
            db.AppSettings.Add(new AppSetting { Key = key, Value = value, UpdatedAt = now });
        }
    }

    /// <summary>
    /// Creates the first admin only from configuration (env vars / user-secrets) and only when no
    /// user exists. No credentials are hardcoded, and nothing is logged beyond the username.
    /// </summary>
    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        if (await db.Users.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var username = configuration["Seed:Admin:Username"];
        var password = configuration["Seed:Admin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No users exist and Seed:Admin:Username/Password are not configured; skipping admin seed");
            return;
        }

        db.Users.Add(new User
        {
            Username = username.Trim(),
            PasswordHash = passwordHasher.Hash(password),
            FirstName = configuration["Seed:Admin:FirstName"] ?? "Admin",
            LastName = configuration["Seed:Admin:LastName"] ?? "NS Store",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = clock.GetUtcNow()
        });

        logger.LogInformation("Seeded initial admin user {Username}", username);
    }
}
