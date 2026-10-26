public static class PeruClock
{
    private static readonly TimeSpan PeruUtcOffset = TimeSpan.FromHours(-5);

    public static DateTime Now => DateTime.UtcNow + PeruUtcOffset;
}
