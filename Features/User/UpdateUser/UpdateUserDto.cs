public record UpdateUserRequestDto(
    int Id,
    string Name,
    string UserName,
    string Password,
    string Email,
    string Role,
    string Status,
    long? EmployeeId = null
);