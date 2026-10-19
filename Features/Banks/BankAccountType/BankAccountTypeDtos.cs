public record CreateBankAccountTypeRequest(
    string Name
);

public record UpdateBankAccountTypeRequest(
    string Name,
    bool IsActive
);

public record BankAccountTypeResponse(
    int Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record BankAccountTypeLookupResponse(
    int Id,
    string Name
);
