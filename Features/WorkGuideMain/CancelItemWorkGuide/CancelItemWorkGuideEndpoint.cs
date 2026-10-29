using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class CancelItemWorkGuideEndpoint
{
    public static void MapCancelItemWorkGuide(this IEndpointRouteBuilder app)
    {
        app.MapPut("/cancelItem/{id}", async (RecepcionDbContext db, int id, ClaimsPrincipal principal, IAppLogger<string> logger) =>
        {
            var workGuideItem = await db.WorkGuideDetails
                .Include(x => x.WorkGuideMain)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (workGuideItem == null || workGuideItem.WorkGuideMain is null)
            {
                logger.LogWarning("Item no encontrado" + id.ToString(), "CancelItemWorkGuideEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "Item no encontrado",
                    StatusCode = 404,
                    Success = false
                };
                return Results.NotFound(responseValidation);
            }

            if (workGuideItem.EstadoRegistro == "I")
            {
                logger.LogWarning("El item ya se encuentra anulado" + id.ToString(), "CancelItemWorkGuideEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "El item ya se encuentra anulado",
                    StatusCode = 400,
                    Success = false
                };
                return Results.BadRequest(responseValidation);
            }

            if (WorkGuideModificationGuard.IsBlocked(workGuideItem.WorkGuideMain.EstadoPago))
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Data = "",
                    Message = "No se puede modificar una guía pagada o anulada.",
                    StatusCode = 400,
                    Success = false
                });
            }

            workGuideItem.EstadoRegistro = "I";

            db.WorkGuideDetailChangeLogs.Add(new WorkGuideDetailChangeLog
            {
                WorkGuideMainId = workGuideItem.WorkGuideMainId,
                WorkGuideDetailId = workGuideItem.Id,
                ChangeType = WorkGuideDetailChangeType.Removed,
                ServiceItemId = workGuideItem.ProductId,
                OldQuantity = workGuideItem.Cant,
                UserId = principal.GetUserId() ?? 0,
                CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"))
            });

            // El recalculo suma Total vía SQL, así que la anulación de arriba debe quedar
            // guardada antes de leerla de vuelta (si no, el ítem anulado se seguiría sumando).
            await db.SaveChangesAsync();
            await WorkGuideTotalsRecalculator.RecalculateAsync(db, workGuideItem.WorkGuideMainId);
            await db.SaveChangesAsync();

            var response = new ApiResponse<string>()
            {
                Data = "El item fue anulado con exito",
                Message = "El item fue anulado con exito",
                Success = true,
                StatusCode = 200
            };

            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionEmision);
    }
}
