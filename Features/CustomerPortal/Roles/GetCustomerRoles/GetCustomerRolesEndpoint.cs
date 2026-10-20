using Microsoft.EntityFrameworkCore;

public static class GetCustomerRolesEndpoint
{
    public static void MapGetCustomerRoles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/customer-roles", async (RecepcionDbContext db) =>
        {
            var roles = await db.CustomerRoles
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new CustomerRoleResponse(x.Id, x.Name, x.Description))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<CustomerRoleResponse>>
            {
                Success = true,
                Message = "Listado de roles disponibles.",
                StatusCode = StatusCodes.Status200OK,
                Data = roles
            });
        })
        .WithName("GetCustomerRoles")
        .Produces<ApiResponse<List<CustomerRoleResponse>>>(StatusCodes.Status200OK)
        .RequireCustomerPermission(CustomerPermissionCodes.UsersManage);
    }
}
