public record CustomerMeResponse(int CustomerId, long CustomerUserId, string Role, string CompanyName, IReadOnlyList<string> Permissions);
