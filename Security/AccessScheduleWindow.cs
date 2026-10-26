using System.Globalization;

public record AccessScheduleWindow(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime)
{
    public const string ClaimType = "accessWindow";

    public string ToClaimValue() =>
        $"{(int)DayOfWeek}:{StartTime.ToString("HH:mm", CultureInfo.InvariantCulture)}-{EndTime.ToString("HH:mm", CultureInfo.InvariantCulture)}";

    public bool Contains(DayOfWeek day, TimeOnly time) =>
        DayOfWeek == day && time >= StartTime && time <= EndTime;

    public static AccessScheduleWindow? TryParse(string value)
    {
        var dayAndRange = value.Split(':', 2);
        if (dayAndRange.Length != 2 || !int.TryParse(dayAndRange[0], out var dayValue) || dayValue < 0 || dayValue > 6)
        {
            return null;
        }

        var range = dayAndRange[1].Split('-');
        if (range.Length != 2)
        {
            return null;
        }

        if (!TimeOnly.TryParseExact(range[0], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
            !TimeOnly.TryParseExact(range[1], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
        {
            return null;
        }

        return new AccessScheduleWindow((DayOfWeek)dayValue, start, end);
    }
}
