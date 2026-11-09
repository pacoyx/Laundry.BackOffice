using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class GetCompanyWorkersEndpoint
{
    public static void MapGetCompanyWorkers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/workers", async (
            ClaimsPrincipal principal,
            [FromQuery] bool? onlyActive,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var query = db.CompanyWorkers
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId.Value);

            if (onlyActive == true)
            {
                query = query.Where(x => x.IsActive);
            }

            var workers = await query
                .OrderBy(x => x.FullName)
                .Select(x => CompanyWorkersEndpoints.ToResponse(x))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<CompanyWorkerResponse>>
            {
                Success = true,
                Message = "Listado de trabajadores de la empresa.",
                StatusCode = StatusCodes.Status200OK,
                Data = workers
            });
        })
        .WithName("GetCompanyWorkers")
        .Produces<ApiResponse<List<CompanyWorkerResponse>>>(StatusCodes.Status200OK)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}
