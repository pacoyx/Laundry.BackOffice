public record CreateCompanyBankAccountRequest(
    int BankId,
    int BankAccountTypeId,
    string CurrencyId,
    string AccountNumber,
    string? InterbankAccountNumber,
    string? Description,
    string? Notes
);

public record UpdateCompanyBankAccountRequest(
    int BankId,
    int BankAccountTypeId,
    string CurrencyId,
    string AccountNumber,
    string? InterbankAccountNumber,
    string? Description,
    string? Notes
);

public record CompanyBankAccountResponse(
    int Id,
    int CompanyId,
    int BankId,
    string BankName,
    int BankAccountTypeId,
    string BankAccountTypeName,
    string CurrencyId,
    string AccountNumber,
    string? InterbankAccountNumber,
    string? Description,
    bool IsEnabled,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
