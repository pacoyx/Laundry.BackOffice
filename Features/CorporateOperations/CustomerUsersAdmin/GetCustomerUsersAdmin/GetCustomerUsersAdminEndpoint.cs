using Microsoft.EntityFrameworkCore;

public static class GetCustomerUsersAdminEndpoint
{
    public static void MapGetCustomerUsersAdmin(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (int? customerId, string? userName, RecepcionDbContext db) =>
        {
            if (customerId is null && string.IsNullOrWhiteSpace(userName))
            {
                return Results.BadRequest(new[] { "Debe especificar customerId o userName para la búsqueda." });
            }

            var query = db.CustomerUsers
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.CustomerRole)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(x => x.UserName.Contains(userName));
            }

            var users = await query.OrderBy(x => x.FullName).ToListAsync();

            var response = users.Select(u => new CustomerUserAdminResponse(
                u.Id, u.FullName, u.Email, u.UserName, u.CustomerId,
                PickupGuidesEndpoints.GetCustomerDisplayName(u.Customer),
                u.CustomerRole.Name, u.IsActive, u.CreatedAt, u.LastLoginAt
            )).ToList();

            return Results.Ok(new ApiResponse<List<CustomerUserAdminResponse>>
            {
                Success = true,
                Message = "Listado de usuarios del portal de clientes.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetCustomerUsersAdmin")
        .Produces<ApiResponse<List<CustomerUserAdminResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequirePermission(PermissionCodes.CorporateOperations);
    }
}
