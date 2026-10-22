public record ScheduledEmailRecipientResponse(
    int Id,
    string Email,
    ScheduledEmailRecipientType RecipientType,
    DateTime CreatedAt
);

// DTO optimizado para listados en Angular: horario ya resuelto a campos simples
// (Hour, Minute, DaysOfWeek) en vez de obligar al frontend a interpretar CronExpression,
// e info de ejecucion ya calculada para no depender del Dashboard de Hangfire.
public record ScheduledEmailNotificationSummaryResponse(
    int Id,
    string Code,
    string Name,
    ScheduledEmailRecipientMode RecipientMode,
    int Hour,
    int Minute,
    List<ScheduledWeekday> DaysOfWeek,
    string CronExpression,
    bool IsEnabled,
    int RecipientsCount,
    DateTime? LastExecutionAt,
    DateTime? NextExecutionAt,
    int ExecutionCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// DTO de detalle: configuracion + destinatarios + info de ejecucion en una sola respuesta,
// para que la pantalla de edicion de Angular no necesite múltiples llamadas.
public record ScheduledEmailNotificationResponse(
    int Id,
    string Code,
    string Name,
    ScheduledEmailRecipientMode RecipientMode,
    int Hour,
    int Minute,
    List<ScheduledWeekday> DaysOfWeek,
    string CronExpression,
    string SubjectTemplate,
    string BodyTemplate,
    string? DriverName,
    string? DriverDocument,
    string SignatureName,
    string SignaturePosition,
    string SignaturePhone,
    bool IsEnabled,
    DateTime? LastExecutionAt,
    DateTime? NextExecutionAt,
    int ExecutionCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    List<ScheduledEmailRecipientResponse> Recipients
);

public record ScheduledEmailExecutionResponse(
    long Id,
    DateTime ExecutedAt,
    ScheduledEmailExecutionStatus Status,
    string? ErrorMessage,
    string Recipients,
    DateTime CreatedAt
);

public record ScheduledEmailPreviewResponse(string Subject, string Body);
