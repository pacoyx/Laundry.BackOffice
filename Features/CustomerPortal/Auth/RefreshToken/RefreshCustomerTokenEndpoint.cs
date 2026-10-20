using Microsoft.EntityFrameworkCore;

public static class RefreshCustomerTokenEndpoint
{
    public static void MapRefreshCustomerToken(this IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh-token", async (
            RefreshCustomerTokenRequest request,
            RecepcionDbContext db,
            JwtService jwtService) =>
        {
            long? customerUserId;
            try
            {
                var principal = jwtService.GetPrincipalFromExpiredToken(request.Token);
                customerUserId = principal.GetCustomerUserId();
            }
            catch (Exception)
            {
                return Results.Unauthorized();
            }

            if (customerUserId is null)
            {
                return Results.Unauthorized();
            }

            var user = await db.CustomerUsers
                .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.CustomerRole).ThenInclude(r => r.Permissions).ThenInclude(p => p.CustomerPermission)
                .FirstOrDefaultAsync(x => x.Id == customerUserId);

            if (user is null || !user.IsActive || user.RefreshToken != request.RefreshToken)
            {
                return Results.Unauthorized();
            }

            var permissions = user.CustomerRole.Permissions.Select(p => p.CustomerPermission.Code).ToList();
            var newToken = jwtService.GenerateCustomerToken(user.Id, user.CustomerId, user.UserName, user.CustomerRole.Name, permissions);
            var newRefreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await db.SaveChangesAsync();

            var companyName = PickupGuidesEndpoints.GetCustomerDisplayName(user.Customer);

            return Results.Ok(new ApiResponse<LoginCustomerResponse>
            {
                Success = true,
                Message = "Token renovado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new LoginCustomerResponse(
                    newToken,
                    newRefreshToken,
                    new CustomerMeResponse(user.CustomerId, user.Id, user.CustomerRole.Name, companyName, permissions))
            });
        })
        .WithName("RefreshCustomerPortalToken")
        .Produces<ApiResponse<LoginCustomerResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
