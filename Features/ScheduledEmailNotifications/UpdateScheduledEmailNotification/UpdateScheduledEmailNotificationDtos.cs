public record UpdateScheduledEmailNotificationRequest(
    string Name,
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
