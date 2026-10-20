public record CustomerUserAdminResponse(
    long Id,
    string FullName,
    string Email,
    string UserName,
    int CustomerId,
    string CustomerName,
    string RoleName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
