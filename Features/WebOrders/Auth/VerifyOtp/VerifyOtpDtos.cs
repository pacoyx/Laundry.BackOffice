public record VerifyOtpRequest(string Phone, string Code, string? Name, int? DistrictId);

public record WebCustomerMeResponse(int CustomerId, string Name, string? Phone, int? DistrictId);

public record VerifyOtpResponse(string Token, string RefreshToken, WebCustomerMeResponse Customer);
