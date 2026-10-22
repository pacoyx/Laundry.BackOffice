public record CustomerBillingProfileRequestDto(
    string TaxId,
    string BusinessName,
    string? CommercialName,
    string FiscalAddress,
    string? BillingEmail,
    int PaymentTermDays,
    decimal CreditLimit,
    bool IsCreditEnabled
);

public record CustomerBillingProfileResponseDto(
    int CustomerId,
    string TaxId,
    string BusinessName,
    string? CommercialName,
    string FiscalAddress,
    string? BillingEmail,
    int PaymentTermDays,
    decimal CreditLimit,
    bool IsCreditEnabled,
    bool RemindersEnabled,
    string? RemindersSuppressedReason
);
