namespace FinTracker.Application.Helpers;

/// <summary>
/// PostgreSQL (Npgsql) требует DateTime с Kind=Utc для timestamptz.
/// </summary>
public static class UtcDateHelper
{
    public static DateTime ToUtcDate(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value.Date,
            DateTimeKind.Local => value.ToUniversalTime().Date,
            _ => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc)
        };
    }

    public static DateTime? ToUtcDate(DateTime? value) =>
        value.HasValue ? ToUtcDate(value.Value) : null;

    public static DateTime StartOfMonthUtc(DateTime? reference = null)
    {
        var refDate = ToUtcDate(reference ?? DateTime.UtcNow);
        return new DateTime(refDate.Year, refDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public static DateTime EndOfMonthUtc(DateTime? reference = null)
    {
        return StartOfMonthUtc(reference).AddMonths(1).AddTicks(-1);
    }

    public static (DateTime From, DateTime To) CurrentMonthRangeUtc()
    {
        var from = StartOfMonthUtc();
        var to = EndOfMonthUtc();
        return (from, to);
    }

    public static (int Year, int Month) CurrentYearMonthUtc()
    {
        var today = ToUtcDate(DateTime.UtcNow);
        return (today.Year, today.Month);
    }

    public static DateTime TodayUtc() => ToUtcDate(DateTime.UtcNow);
}
