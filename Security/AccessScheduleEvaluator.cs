public static class AccessScheduleEvaluator
{
    public static bool IsWithinSchedule(IReadOnlyCollection<AccessScheduleWindow> windows, DateTime nowPeru)
    {
        if (windows.Count == 0)
        {
            return true;
        }

        var day = nowPeru.DayOfWeek;
        var time = TimeOnly.FromDateTime(nowPeru);

        return windows.Any(w => w.Contains(day, time));
    }
}
