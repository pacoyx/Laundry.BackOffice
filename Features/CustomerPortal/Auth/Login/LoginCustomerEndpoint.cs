using Microsoft.EntityFrameworkCore;

public static class LoginCustomerEndpoint
{
    public static void MapLoginCustomer(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (
            LoginCustomerRequest request,
            RecepcionDbContext db,
            JwtService jwtService,
            IEncryptService encryptService) =>
        {
            var user = await db.CustomerUsers
                .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.CustomerRole).ThenInclude(r => r.Permissions).ThenInclude(p => p.CustomerPermission)
                .FirstOrDefaultAsync(x => x.UserName == request.UserName);

            if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !encryptService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            if (!user.IsActive)
            {
                return Results.BadRequest(new[] { "El usuario se encuentra inactivo." });
            }

            if (!user.CustomerRole.IsActive)
            {
                return Results.BadRequest(new[] { "El rol asignado al usuario se encuentra inactivo." });
            }

            var permissions = user.CustomerRole.Permissions.Select(p => p.CustomerPermission.Code).ToList();
            var token = jwtService.GenerateCustomerToken(user.Id, user.CustomerId, user.UserName, user.CustomerRole.Name, permissions);
            var refreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.LastLoginAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            await db.SaveChangesAsync();

            var companyName = PickupGuidesEndpoints.GetCustomerDisplayName(user.Customer);

            var response = new ApiResponse<LoginCustomerResponse>
            {
                Success = true,
                Message = "Inicio de sesión correcto.",
                StatusCode = StatusCodes.Status200OK,
                Data = new LoginCustomerResponse(
                    token,
                    refreshToken,
                    new CustomerMeResponse(user.CustomerId, user.Id, user.CustomerRole.Name, companyName, permissions))
            };

            return Results.Ok(response);
        })
        .WithName("LoginCustomerPortal")
        .Produces<ApiResponse<LoginCustomerResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
