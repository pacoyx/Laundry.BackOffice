using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class DeactivateCompanyWorkerEndpoint
{
    public static void MapDeactivateCompanyWorker(this IEndpointRouteBuilder app)
    {
        app.MapPost("/workers/{id:int}/deactivate", async (int id, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var worker = await db.CompanyWorkers
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (worker is null)
            {
                return Results.NotFound();
            }

            worker.IsActive = !worker.IsActive;
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = worker.IsActive ? "Trabajador activado correctamente." : "Trabajador desactivado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { worker.Id, worker.IsActive }
            });
        })
        .WithName("DeactivateCompanyWorker")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}
