public record InvoiceReminderLogDetailResponse(
    long InvoiceId,
    string DocumentNumber,
    decimal Balance,
    int DaysOverdue
);

public record InvoiceReminderLogResponse(
    long Id,
    int CustomerId,
    string CustomerName,
    DateTime RunAt,
    InvoiceReminderLogStatus Status,
    string? SkipReason,
    string? ErrorMessage,
    string Recipients,
    int InvoiceCount,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<InvoiceReminderLogDetailResponse> Details
);

public record InvoiceReminderPreviewResponse(string Subject, string Body);
