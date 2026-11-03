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
                .Include(x => x.Product)
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

            var hasNewQuantity = request.NewQuantity.HasValue;
            var hasNewTotal = request.NewTotal.HasValue;

            if (hasNewQuantity == hasNewTotal)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Debe enviar exactamente uno: la nueva cantidad o el nuevo total.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var oldQuantity = detail.Cant;
            var oldPrecio = detail.Precio;
            decimal? newPrecioForLog = null;

            if (hasNewQuantity)
            {
                if (request.NewQuantity!.Value <= 0)
                {
                    return Results.BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "La cantidad debe ser mayor a cero.",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                detail.Cant = request.NewQuantity.Value;
                detail.Total = detail.Precio * request.NewQuantity.Value;
            }
            else
            {
                var newTotal = request.NewTotal!.Value;
                if (newTotal <= 0)
                {
                    return Results.BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "El total debe ser mayor a cero.",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Servicio por peso: el precio unitario (por kilo) es fijo, se recalcula la
                // cantidad (kilos) a partir del total ingresado. Servicio por unidad: la
                // cantidad es fija (siempre es un conteo de piezas), se recalcula el precio
                // unitario a partir del total ingresado.
                var esPorPeso = detail.Product?.IsPeso == "S";
                if (esPorPeso)
                {
                    if (detail.Precio <= 0)
                    {
                        return Results.BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "El precio unitario del servicio no es válido para recalcular la cantidad.",
                            StatusCode = StatusCodes.Status400BadRequest
                        });
                    }

                    detail.Cant = Math.Round(newTotal / detail.Precio, 2);
                    detail.Total = newTotal;
                }
                else
                {
                    if (detail.Cant <= 0)
                    {
                        return Results.BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "La cantidad actual del ítem no es válida para recalcular el precio.",
                            StatusCode = StatusCodes.Status400BadRequest
                        });
                    }

                    newPrecioForLog = Math.Round(newTotal / detail.Cant, 2);
                    detail.Precio = newPrecioForLog.Value;
                    detail.Total = newTotal;
                }
            }

            db.WorkGuideDetailChangeLogs.Add(new WorkGuideDetailChangeLog
            {
                WorkGuideMainId = detail.WorkGuideMainId,
                WorkGuideDetailId = detail.Id,
                ChangeType = newPrecioForLog.HasValue ? WorkGuideDetailChangeType.PriceAdjusted : WorkGuideDetailChangeType.QuantityAdjusted,
                ServiceItemId = detail.ProductId,
                OldQuantity = oldQuantity,
                NewQuantity = detail.Cant,
                OldPrice = newPrecioForLog.HasValue ? oldPrecio : null,
                NewPrice = newPrecioForLog,
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
                Message = "Ítem actualizado.",
                StatusCode = StatusCodes.Status200OK,
                Data = new AdjustWorkGuideItemQuantityResponse(detail.Id, detail.Cant, detail.Precio, detail.Total, detail.WorkGuideMain.Total)
            });
        })
        .WithName("AdjustWorkGuideItemQuantity")
        .Produces<ApiResponse<AdjustWorkGuideItemQuantityResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionEmision);
    }
}
