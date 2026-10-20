public record ResetCustomerUserPasswordAdminRequest(string NewPassword);

public record ResetCustomerUserPasswordAdminResponse(
    long Id,
    string UserName,
    string Email,
    int CustomerId,
    string CustomerName
);
