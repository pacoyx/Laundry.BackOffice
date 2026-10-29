public record GetUserResponseDto(
    int Id,
    string Name,
    string UserName,
    string Email,
    string Role,
    string Status,
    long? EmployeeId
);