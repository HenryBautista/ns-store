using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Settings;

/// <summary>
/// Business parameters as percentages (e.g. <c>vatRate = 16</c> means 16%).
/// <paramref name="OverdueDays"/> is a day count, not a percentage.
/// </summary>
public record SettingsDto(decimal VatRate, decimal DefaultMarginPct, string Currency, int OverdueDays);

public record UpdateSettingsRequest(decimal VatRate, decimal DefaultMarginPct, string Currency, int OverdueDays);

public class SettingsService(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    /// <summary>Legacy rule 7 ("deuda vencida > 15 días"), now the starting value rather than the law.</summary>
    public const int DefaultOverdueDays = 15;

    public async Task<SettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await db.AppSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return new SettingsDto(
            ReadDecimal(settings, AppSettingKeys.VatRate, 16m),
            ReadDecimal(settings, AppSettingKeys.DefaultMarginPct, 30m),
            settings.GetValueOrDefault(AppSettingKeys.Currency, "BOB"),
            ReadInt(settings, AppSettingKeys.OverdueDays, DefaultOverdueDays));
    }

    public async Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        await UpsertAsync(AppSettingKeys.VatRate, request.VatRate.ToString(CultureInfo.InvariantCulture), cancellationToken);
        await UpsertAsync(AppSettingKeys.DefaultMarginPct, request.DefaultMarginPct.ToString(CultureInfo.InvariantCulture), cancellationToken);
        await UpsertAsync(AppSettingKeys.Currency, request.Currency.Trim().ToUpperInvariant(), cancellationToken);
        await UpsertAsync(AppSettingKeys.OverdueDays, request.OverdueDays.ToString(CultureInfo.InvariantCulture), cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(cancellationToken);
    }

    private async Task UpsertAsync(string key, string value, CancellationToken cancellationToken)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        if (setting is null)
        {
            setting = new AppSetting { Key = key };
            db.AppSettings.Add(setting);
        }

        setting.Value = value;
        setting.UpdatedBy = currentUser.UserId;
        setting.UpdatedAt = clock.GetUtcNow();
    }

    private static decimal ReadDecimal(IReadOnlyDictionary<string, string> settings, string key, decimal fallback) =>
        settings.TryGetValue(key, out var raw) && decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;

    private static int ReadInt(IReadOnlyDictionary<string, string> settings, string key, int fallback) =>
        settings.TryGetValue(key, out var raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
}
