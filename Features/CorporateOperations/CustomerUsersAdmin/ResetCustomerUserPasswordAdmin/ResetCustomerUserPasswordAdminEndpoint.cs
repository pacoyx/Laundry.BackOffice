using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class ResetCustomerUserPasswordAdminEndpoint
{
    public static void MapResetCustomerUserPasswordAdmin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/reset-password", async (
            long id,
            ResetCustomerUserPasswordAdminRequest request,
            IValidator<ResetCustomerUserPasswordAdminRequest> validator,
            RecepcionDbContext db,
            IEncryptService encryptService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            // Sin filtro por CustomerId: lo llama el staff interno, no un admin del portal de clientes.
            var user = await db.CustomerUsers
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user is null)
            {
                return Results.NotFound();
            }

            var pendingTokens = await db.CustomerUserTokens
                .Where(x => x.CustomerUserId == id && x.Purpose == CustomerUserTokenPurpose.PasswordReset && !x.IsUsed)
                .ToListAsync();
            foreach (var pending in pendingTokens)
            {
                pending.IsUsed = true;
            }

            user.PasswordHash = encryptService.HashPassword(request.NewPassword);
            user.RefreshToken = null;
            await db.SaveChangesAsync();

            var response = new ResetCustomerUserPasswordAdminResponse(
                user.Id, user.UserName, user.Email, user.CustomerId,
                PickupGuidesEndpoints.GetCustomerDisplayName(user.Customer));

            return Results.Ok(new ApiResponse<ResetCustomerUserPasswordAdminResponse>
            {
                Success = true,
                Message = "Contraseña actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("ResetCustomerUserPasswordAdmin")
        .Produces<ApiResponse<ResetCustomerUserPasswordAdminResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.CorporateOperations);
    }
}
