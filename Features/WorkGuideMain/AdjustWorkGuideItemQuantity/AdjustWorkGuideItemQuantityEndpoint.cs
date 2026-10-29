using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class AdjustWorkGuideItemQuantityEndpoint
{
    public static void MapAdjustWorkGuideItemQuantity(this IEndpointRouteBuilder app)
    {
        app.MapPut("/items/{detailId:int}/quantity", async (int detailId, AdjustWorkGuideItemQuantityRequest request, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var detail = await db.WorkGuideDetails
                .Include(x => x.WorkGuideMain)
                .FirstOrDefaultAsync(x => x.Id == detailId);

            if (detail is null || detail.WorkGuideMain is null)
            {
                return Results.NotFound();
            }

            if (WorkGuideModificationGuard.IsBlocked(detail.WorkGuideMain.EstadoPago))
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se puede modificar una guía pagada o anulada.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (detail.EstadoRegistro != "A")
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El ítem se encuentra anulado.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (request.NewQuantity <= 0)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "La cantidad debe ser mayor a cero.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var oldQuantity = detail.Cant;
            detail.Cant = request.NewQuantity;
            detail.Total = detail.Precio * request.NewQuantity;

            db.WorkGuideDetailChangeLogs.Add(new WorkGuideDetailChangeLog
            {
                WorkGuideMainId = detail.WorkGuideMainId,
                WorkGuideDetailId = detail.Id,
                ChangeType = WorkGuideDetailChangeType.QuantityAdjusted,
                ServiceItemId = detail.ProductId,
                OldQuantity = oldQuantity,
                NewQuantity = request.NewQuantity,
                Observation = request.Observation,
                UserId = principal.GetUserId() ?? 0,
                CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"))
            });

            // El recalculo suma Total vía SQL, así que la cantidad/Total modificados arriba
            // deben quedar guardados antes de leerlos de vuelta.
            await db.SaveChangesAsync();
            await WorkGuideTotalsRecalculator.RecalculateAsync(db, detail.WorkGuideMainId);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<AdjustWorkGuideItemQuantityResponse>
            {
                Success = true,
                Message = "Cantidad actualizada.",
                StatusCode = StatusCodes.Status200OK,
                Data = new AdjustWorkGuideItemQuantityResponse(detail.Id, detail.WorkGuideMain.Total)
            });
        })
        .WithName("AdjustWorkGuideItemQuantity")
        .Produces<ApiResponse<AdjustWorkGuideItemQuantityResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionEmision);
    }
}
