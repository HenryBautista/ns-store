namespace NsStore.Domain.Entities;

/// <summary>
/// Business parameters editable by an admin — replaces the legacy hardcoded 30% margin / 16% VAT.
/// </summary>
public class AppSetting
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public long? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public static class AppSettingKeys
{
    public const string VatRate = "vat_rate";
    public const string DefaultMarginPct = "default_margin_pct";
    public const string Currency = "currency";

    /// <summary>
    /// Days a balance may sit untouched before it counts as overdue. Measured from the last
    /// instalment, or from the sale date when none has been paid.
    /// </summary>
    public const string OverdueDays = "overdue_days";
}
