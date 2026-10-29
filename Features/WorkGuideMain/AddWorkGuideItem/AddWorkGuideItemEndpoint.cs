using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class AddWorkGuideItemEndpoint
{
    public static void MapAddWorkGuideItem(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/items", async (int id, AddWorkGuideItemRequest request, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var workGuideMain = await db.WorkGuideMains.FirstOrDefaultAsync(x => x.Id == id);
            if (workGuideMain is null)
            {
                return Results.NotFound();
            }

            if (WorkGuideModificationGuard.IsBlocked(workGuideMain.EstadoPago))
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se puede modificar una guía pagada o anulada.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var serviceExists = await db.ProdServices.AnyAsync(x => x.Id == request.ServiceItemId);
            if (!serviceExists)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El servicio indicado no existe.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (request.Quantity <= 0)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "La cantidad debe ser mayor a cero.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            // Ubicacion guarda el Name real de un LocationClothes (igual que
            // AssignWorkGuideDetailLocationEndpoint), no un codigo de letra.
            var lavanderia = await db.LocationClothes.FirstOrDefaultAsync(x => x.Name == "Lavandería");

            var detail = new WorkGuideDetail
            {
                Cant = request.Quantity,
                Precio = request.UnitPrice,
                Total = request.Quantity * request.UnitPrice,
                Observaciones = request.Observation ?? "",
                TipoLavado = "",
                Ubicacion = lavanderia?.Name ?? "",
                EstadoTrabajo = "P",
                ProductId = request.ServiceItemId,
                WorkGuideMainId = id,
                EstadoRegistro = "A",
                EstadoSituacion = "P",
                EstadoPago = "PE",
                Identificador = ""
            };

            db.WorkGuideDetails.Add(detail);
            await db.SaveChangesAsync();

            db.WorkGuideDetailChangeLogs.Add(new WorkGuideDetailChangeLog
            {
                WorkGuideMainId = id,
                WorkGuideDetailId = detail.Id,
                ChangeType = WorkGuideDetailChangeType.Added,
                ServiceItemId = request.ServiceItemId,
                NewQuantity = request.Quantity,
                Observation = request.Observation,
                UserId = principal.GetUserId() ?? 0,
                CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"))
            });

            await WorkGuideTotalsRecalculator.RecalculateAsync(db, id);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<AddWorkGuideItemResponse>
            {
                Success = true,
                Message = "Ítem agregado a la guía.",
                StatusCode = StatusCodes.Status200OK,
                Data = new AddWorkGuideItemResponse(detail.Id, workGuideMain.Total)
            });
        })
        .WithName("AddWorkGuideItem")
        .Produces<ApiResponse<AddWorkGuideItemResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionEmision);
    }
}
