// Horario estructurado (Hour, Minute, DaysOfWeek) en vez de un CronExpression crudo:
// el backend arma el cron internamente, el frontend nunca necesita construirlo ni parsearlo.
public record CreateScheduledEmailNotificationRequest(
    string Code,
    string Name,
    ScheduledEmailRecipientMode RecipientMode,
    int Hour,
    int Minute,
    List<ScheduledWeekday> DaysOfWeek,
    string SubjectTemplate,
    string BodyTemplate,
    string? DriverName,
    string? DriverDocument,
    string? SignatureName,
    string? SignaturePosition,
    string? SignaturePhone
);
