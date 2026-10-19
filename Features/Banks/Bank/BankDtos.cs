public record CreateBankRequest(
    string Name,
    string? ShortName
);

public record UpdateBankRequest(
    string Name,
    string? ShortName,
    bool IsActive
);

public record BankResponse(
    int Id,
    string Name,
    string? ShortName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record BankLookupResponse(
    int Id,
    string Name
);
