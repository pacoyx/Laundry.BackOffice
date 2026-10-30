using Microsoft.EntityFrameworkCore;

public static class RefreshWebCustomerTokenEndpoint
{
    public static void MapRefreshWebCustomerToken(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh-token", async (
            RefreshWebCustomerTokenRequest request,
            RecepcionDbContext db,
            JwtService jwtService) =>
        {
            long? webCustomerAccountId;
            try
            {
                var principal = jwtService.GetPrincipalFromExpiredToken(request.Token);
                webCustomerAccountId = principal.GetWebCustomerAccountId();
            }
            catch (Exception)
            {
                return Results.Unauthorized();
            }

            if (webCustomerAccountId is null)
            {
                return Results.Unauthorized();
            }

            var account = await db.WebCustomerAccounts
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == webCustomerAccountId);

            if (account is null || !account.IsActive || account.RefreshToken != request.RefreshToken)
            {
                return Results.Unauthorized();
            }

            var newToken = jwtService.GenerateWebCustomerToken(account.Id, account.CustomerId, account.Phone);
            var newRefreshToken = jwtService.GenerateRefreshToken();

            account.RefreshToken = newRefreshToken;
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<VerifyOtpResponse>
            {
                Success = true,
                Message = "Token renovado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new VerifyOtpResponse(
                    newToken,
                    newRefreshToken,
                    new WebCustomerMeResponse(account.Customer.Id, account.Customer.FirtsName, account.Customer.Phone, account.Customer.DistrictId))
            });
        })
        .WithName("RefreshWebCustomerToken")
        .WithTags("WebOrders")
        .Produces<ApiResponse<VerifyOtpResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
