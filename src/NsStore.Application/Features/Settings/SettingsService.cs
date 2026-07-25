using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NsStore.Application.Common.Interfaces;
using NsStore.Domain.Entities;

namespace NsStore.Application.Features.Settings;

/// <summary>Business parameters as percentages (e.g. <c>vatRate = 16</c> means 16%).</summary>
public record SettingsDto(decimal VatRate, decimal DefaultMarginPct, string Currency);

public record UpdateSettingsRequest(decimal VatRate, decimal DefaultMarginPct, string Currency);

public class SettingsService(IAppDbContext db, ICurrentUser currentUser, TimeProvider clock)
{
    public async Task<SettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await db.AppSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return new SettingsDto(
            ReadDecimal(settings, AppSettingKeys.VatRate, 16m),
            ReadDecimal(settings, AppSettingKeys.DefaultMarginPct, 30m),
            settings.GetValueOrDefault(AppSettingKeys.Currency, "BOB"));
    }

    public async Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        await UpsertAsync(AppSettingKeys.VatRate, request.VatRate.ToString(CultureInfo.InvariantCulture), cancellationToken);
        await UpsertAsync(AppSettingKeys.DefaultMarginPct, request.DefaultMarginPct.ToString(CultureInfo.InvariantCulture), cancellationToken);
        await UpsertAsync(AppSettingKeys.Currency, request.Currency.Trim().ToUpperInvariant(), cancellationToken);

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
}
