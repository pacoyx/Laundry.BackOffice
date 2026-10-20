using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateCustomerUserEndpoint
{
    public static void MapUpdateCustomerUser(this IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{id:long}", async (
            long id,
            ClaimsPrincipal principal,
            UpdateCustomerUserRequest request,
            IValidator<UpdateCustomerUserRequest> validator,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            // Filtrado por CustomerId del token: no permite editar usuarios de otra empresa.
            var user = await db.CustomerUsers
                .Include(x => x.CustomerRole)
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (user is null)
            {
                return Results.NotFound();
            }

            var roleExists = await db.CustomerRoles.AnyAsync(x => x.Id == request.CustomerRoleId && x.IsActive);
            if (!roleExists)
            {
                return Results.BadRequest(new[] { "Rol no encontrado o inactivo." });
            }

            var emailTaken = await db.CustomerUsers.AnyAsync(x => x.Email == request.Email && x.Id != id);
            if (emailTaken)
            {
                return Results.BadRequest(new[] { "El correo ya está registrado." });
            }

            var roleChanged = user.CustomerRoleId != request.CustomerRoleId;

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.CustomerRoleId = request.CustomerRoleId;

            // Si cambia el rol, se invalida la sesión activa para que los nuevos permisos
            // apliquen de inmediato en el siguiente login en vez de esperar a que expire el token.
            if (roleChanged)
            {
                user.RefreshToken = null;
            }

            await db.SaveChangesAsync();

            await db.Entry(user).Reference(x => x.CustomerRole).LoadAsync();

            return Results.Ok(new ApiResponse<CustomerUserResponse>
            {
                Success = true,
                Message = "Usuario actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = CustomerUsersEndpoints.ToResponse(user)
            });
        })
        .WithName("UpdateCustomerPortalUser")
        .Produces<ApiResponse<CustomerUserResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.UsersManage);
    }
}
