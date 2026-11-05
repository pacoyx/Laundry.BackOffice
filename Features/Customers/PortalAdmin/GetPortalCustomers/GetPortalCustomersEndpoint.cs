using Microsoft.EntityFrameworkCore;

public static class GetPortalCustomersEndpoint
{
    public static void MapGetPortalCustomers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/customers", async (RecepcionDbContext db) =>
        {
            var userStats = await db.CustomerUsers
                .AsNoTracking()
                .GroupBy(x => x.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalUsers = g.Count(),
                    ActiveUsers = g.Count(x => x.IsActive),
                    LastLogin = g.Max(x => x.LastLoginAt)
                })
                .ToListAsync();

            // El portal web es para clientes Empresa (facturan con RUC via su perfil de facturacion);
            // los clientes Individual nunca tienen acceso a el, asi que no deben aparecer en este reporte.
            var customers = await db.Customers
                .AsNoTracking()
                .Include(x => x.BillingProfile)
                .Where(x => x.CustomerType == CustomerType.Company)
                .ToListAsync();

            var report = customers
                .Select(c =>
                {
                    var stats = userStats.FirstOrDefault(s => s.CustomerId == c.Id);
                    var lastLogin = stats?.LastLogin;
                    var status = !c.PortalEnabled
                        ? "Sin portal"
                        : lastLogin is null ? "Nunca ingresó" : "Activo";

                    return new PortalCustomerAdminItem(
                        c.Id,
                        PickupGuidesEndpoints.GetCustomerDisplayName(c),
                        c.PortalEnabled,
                        stats?.TotalUsers ?? 0,
                        stats?.ActiveUsers ?? 0,
                        lastLogin,
                        status);
                })
                .OrderByDescending(x => x.PortalEnabled)
                .ThenBy(x => x.CustomerName)
                .ToList();

            return Results.Ok(new ApiResponse<List<PortalCustomerAdminItem>>
            {
                Success = true,
                Message = "Listado de clientes y su estado de portal.",
                StatusCode = StatusCodes.Status200OK,
                Data = report
            });
        })
        .WithName("GetCustomerPortalAdminCustomers")
        .Produces<ApiResponse<List<PortalCustomerAdminItem>>>(StatusCodes.Status200OK);
    }
}
