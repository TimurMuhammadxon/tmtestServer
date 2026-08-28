namespace OnlineTesting.Application.Common;

/// <summary>
/// The platform's audience is in Uzbekistan (fixed UTC+5, no DST).
/// Central place for "local day" logic so streaks, daily activity and admin
/// day/week boundaries don't drift due to the 5-hour offset.
/// </summary>
public static class AppTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(5);

    public static DateTime LocalNow => DateTime.UtcNow + Offset;

    /// <summary>Current calendar date in local (UTC+5) time.</summary>
    public static DateOnly Today => DateOnly.FromDateTime(LocalNow);

    /// <summary>Local calendar date of a UTC timestamp.</summary>
    public static DateOnly LocalDate(DateTime utc) => DateOnly.FromDateTime(utc + Offset);

    /// <summary>UTC instant of local midnight for the given local date (Kind=Utc for Npgsql timestamptz).</summary>
    public static DateTime StartOfDayUtc(DateOnly localDate) =>
        DateTime.SpecifyKind(localDate.ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);
}
