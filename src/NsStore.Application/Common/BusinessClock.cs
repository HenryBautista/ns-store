namespace NsStore.Application.Common;

/// <summary>
/// The store's calendar day.
/// </summary>
/// <remarks>
/// Bolivia sits at a fixed UTC-4 and observes no daylight saving, so the offset is the whole rule.
/// It matters because dates the business owns — a sale date, a payment date, "today" on the
/// dashboard — are calendar days at the counter, not instants. Deriving them from
/// <c>GetUtcNow().UtcDateTime</c> silently rolls the day over at 20:00 local, which is how the
/// dashboard used to lose an evening's sales.
/// </remarks>
public static class BusinessClock
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(-4);

    /// <summary>The calendar day the counter is currently working.</summary>
    public static DateOnly Today(this TimeProvider clock) =>
        DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(Offset).DateTime);
}
