using FluentValidation;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Cierra un mantenimiento que dejó el activo "En Mantenimiento":
/// registra la fecha real de fin y devuelve el activo al estado que tenía antes.
/// </summary>
public static class FinishFixedAssetMaintenanceEndpoint
{
    public static void MapFinishFixedAssetMaintenance(this IEndpointRouteBuilder app)
    {
        app.MapPost("/maintenances/{maintenanceId:long}/finish", async (
            long maintenanceId,
            FinishFixedAssetMaintenanceRequest request,
            IValidator<FinishFixedAssetMaintenanceRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var maintenance = await db.FixedAssetMaintenances
                .Include(m => m.FixedAsset)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == maintenanceId);
            if (maintenance is null)
            {
                return Results.NotFound("Mantenimiento no encontrado.");
            }

            if (!maintenance.PutsAssetOutOfService)
            {
                return Results.BadRequest(new[] { "Este mantenimiento no dejó el activo fuera de servicio; no hay nada que finalizar." });
            }

            if (maintenance.ActualEndDate.HasValue)
            {
                return Results.BadRequest(new[] { "Este mantenimiento ya fue finalizado." });
            }

            if (request.ActualEndDate.Date < maintenance.Date.Date)
            {
                return Results.BadRequest(new[] { "La fecha de fin no puede ser anterior a la fecha de inicio del mantenimiento." });
            }

            maintenance.ActualEndDate = request.ActualEndDate;
            maintenance.ClosingNotes = string.IsNullOrWhiteSpace(request.ClosingNotes) ? null : request.ClosingNotes.Trim();
            if (request.FinalCost.HasValue)
            {
                maintenance.Cost = request.FinalCost.Value;
            }

            // Solo se restaura el estado si el activo sigue en mantenimiento
            // (si entretanto se dio de baja/vendió, se respeta ese estado).
            var asset = maintenance.FixedAsset;
            if (asset.Status == FixedAssetStatus.InMaintenance)
            {
                asset.Status = maintenance.PreviousStatus ?? FixedAssetStatus.Active;
            }

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<FixedAssetMaintenanceResponse>
            {
                Success = true,
                Message = "Mantenimiento finalizado. El activo volvió a estar operativo.",
                StatusCode = StatusCodes.Status200OK,
                Data = FixedAssetsEndpoints.ToMaintenanceResponse(maintenance)
            });
        })
        .WithName("FinishFixedAssetMaintenance")
        .Produces<ApiResponse<FixedAssetMaintenanceResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
