public record CreateEmailAccountRequest(
    string Operacion,
    string Provider,
    string Host,
    int Port,
    string UserName,
    string Password,
    bool EnableSsl
);

public record UpdateEmailAccountRequest(
    string Operacion,
    string Provider,
    string Host,
    int Port,
    string UserName,
    string? Password,
    bool EnableSsl,
    bool IsActive
);

public record EmailAccountResponse(
    int Id,
    string Operacion,
    string Provider,
    string Host,
    int Port,
    string UserName,
    bool EnableSsl,
    bool IsActive,
    bool HasPassword,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
