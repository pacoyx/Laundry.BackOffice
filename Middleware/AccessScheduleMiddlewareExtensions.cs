public static class AccessScheduleMiddlewareExtensions
{
    public static IApplicationBuilder UseAccessSchedule(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AccessScheduleMiddleware>();
    }
}
