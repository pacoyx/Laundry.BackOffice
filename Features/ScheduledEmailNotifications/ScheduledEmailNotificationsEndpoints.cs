using Cronos;

public static class ScheduledEmailNotificationsEndpoints
{
    public static void MapScheduledEmailNotifications(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/scheduled-email-notifications")
            .WithTags("ScheduledEmailNotifications")
            .RequirePermission(PermissionCodes.Settings);

        group.MapGetScheduledEmailNotifications();
        group.MapGetScheduledEmailNotification();
        group.MapCreateScheduledEmailNotification();
        group.MapUpdateScheduledEmailNotification();
        group.MapEnableScheduledEmailNotification();
        group.MapDisableScheduledEmailNotification();

        group.MapAddScheduledEmailRecipient();
        group.MapDeleteScheduledEmailRecipient();

        group.MapExecuteScheduledEmailNotification();
        group.MapSendTestScheduledEmailNotification();
        group.MapPreviewScheduledEmailNotification();
        group.MapGetScheduledEmailExecutions();
    }

    internal static ScheduledEmailNotificationSummaryResponse ToSummaryResponse(ScheduledEmailNotification config, DateTime? lastExecutionAt, int executionCount)
    {
        var (hour, minute, daysOfWeek) = CronScheduleHelper.ParseCronExpression(config.CronExpression);

        return new ScheduledEmailNotificationSummaryResponse(
            config.Id, config.Code, config.Name, config.RecipientMode, hour, minute, daysOfWeek, config.CronExpression,
            config.IsEnabled, config.Recipients.Count,
            lastExecutionAt, ComputeNextExecutionAt(config), executionCount,
            config.CreatedAt, config.UpdatedAt
        );
    }

    internal static ScheduledEmailNotificationResponse ToResponse(ScheduledEmailNotification config, DateTime? lastExecutionAt, int executionCount)
    {
        var (hour, minute, daysOfWeek) = CronScheduleHelper.ParseCronExpression(config.CronExpression);

        return new ScheduledEmailNotificationResponse(
            config.Id, config.Code, config.Name, config.RecipientMode, hour, minute, daysOfWeek, config.CronExpression,
            config.SubjectTemplate, config.BodyTemplate,
            config.DriverName, config.DriverDocument,
            config.SignatureName, config.SignaturePosition, config.SignaturePhone,
            config.IsEnabled,
            lastExecutionAt, ComputeNextExecutionAt(config), executionCount,
            config.CreatedAt, config.UpdatedAt, config.CreatedBy, config.UpdatedBy,
            config.Recipients
                .OrderBy(r => r.RecipientType).ThenBy(r => r.Email)
                .Select(r => new ScheduledEmailRecipientResponse(r.Id, r.Email, r.RecipientType, r.CreatedAt))
                .ToList()
        );
    }

    internal static ScheduledEmailExecutionResponse ToExecutionResponse(ScheduledEmailExecution execution) => new(
        execution.Id, execution.ExecutedAt, execution.Status, execution.ErrorMessage, execution.Recipients, execution.CreatedAt
    );

    // Calculada analiticamente con Cronos (misma libreria que usa Hangfire por dentro), sin
    // depender de leer el estado interno del storage de Hangfire. Si esta deshabilitada no hay
    // una proxima ejecucion real, por eso es null en ese caso.
    internal static DateTime? ComputeNextExecutionAt(ScheduledEmailNotification config)
    {
        if (!config.IsEnabled)
        {
            return null;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        var cron = CronExpression.Parse(config.CronExpression);
        var next = cron.GetNextOccurrence(DateTimeOffset.UtcNow, timeZone);

        return next?.DateTime;
    }
}
