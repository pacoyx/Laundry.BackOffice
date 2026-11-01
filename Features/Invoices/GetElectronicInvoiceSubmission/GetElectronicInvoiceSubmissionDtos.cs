public record ElectronicInvoiceSubmissionResponse(
    long Id,
    long InvoiceId,
    string Provider,
    DateTime SentAt,
    bool Success,
    bool? AcceptedBySunat,
    string? SunatDescription,
    string? SunatResponseCode,
    string? DocumentUrl,
    string? ErrorMessage
);
