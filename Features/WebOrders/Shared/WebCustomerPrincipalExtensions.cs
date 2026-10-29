using System.Security.Claims;

public static class WebCustomerPrincipalExtensions
{
    public static long? GetWebCustomerAccountId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("webCustomerAccountId")?.Value;
        return long.TryParse(value, out var id) ? id : null;
    }

    public static int? GetWebCustomerCustomerId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("customerId")?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }
}
