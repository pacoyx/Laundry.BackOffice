using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class CreateWebOrderPickupPhotoEndpoint
{
    public static void MapCreateWebOrderPickupPhoto(this IEndpointRouteBuilder app)
    {
        app.MapPost("/web-orders/{id:long}/photos", async (
            long id,
            IFormFile file,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IFileStorageService storage) =>
        {
            var employeeId = principal.GetEmployeeId();
            if (employeeId is null)
            {
                return Results.BadRequest(new[] { "El usuario no está vinculado a un empleado. Contacte al administrador para habilitar el acceso móvil." });
            }

            var webOrder = await db.WebOrders.FirstOrDefaultAsync(x => x.Id == id && x.EmployeeId == employeeId.Value);
            if (webOrder is null || webOrder.WorkGuideMainId is null)
            {
                return Results.NotFound();
            }

            return await CreateItemPhotoEndpoint.ExecuteAsync(
                webOrder.WorkGuideMainId.Value, file, db, storage, WorkGuideEvidenceStage.Recojo, employeeId.Value);
        })
        .DisableAntiforgery()
        .WithName("CreateWebOrderPickupPhoto")
        .Produces<ApiResponse<ItemPhotoResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
