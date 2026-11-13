using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateFixedAssetMaintenanceEndpoint
{
    public static void MapCreateFixedAssetMaintenance(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/maintenances", async (
            long id,
            CreateFixedAssetMaintenanceRequest request,
            IValidator<CreateFixedAssetMaintenanceRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var asset = await db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id);
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            if (asset.Status is FixedAssetStatus.Disposed or FixedAssetStatus.Sold)
            {
                return Results.BadRequest(new[] { "No se puede registrar mantenimiento a un activo dado de baja o vendido." });
            }

            if (request.PutAssetOutOfService && asset.Status == FixedAssetStatus.InMaintenance)
            {
                return Results.BadRequest(new[] { "El activo ya se encuentra en mantenimiento. Finaliza el mantenimiento en curso antes de iniciar otro." });
            }

            var maintenance = new FixedAssetMaintenance
            {
                FixedAssetId = id,
                Date = request.Date,
                Type = request.Type,
                Description = request.Description,
                Cost = request.Cost,
                Observations = request.Observations,
                NextMaintenanceDate = request.NextMaintenanceDate,
                ServiceProvider = string.IsNullOrWhiteSpace(request.ServiceProvider) ? null : request.ServiceProvider.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            if (request.PutAssetOutOfService)
            {
                maintenance.PutsAssetOutOfService = true;
                maintenance.OutOfServiceReason = request.OutOfServiceReason!.Trim();
                maintenance.EstimatedEndDate = request.EstimatedEndDate;
                maintenance.PreviousStatus = asset.Status;
                asset.Status = FixedAssetStatus.InMaintenance;
            }

            await db.FixedAssetMaintenances.AddAsync(maintenance);
            await db.SaveChangesAsync();

            var response = new ApiResponse<FixedAssetMaintenanceResponse>
            {
                Success = true,
                Message = request.PutAssetOutOfService
                    ? "Mantenimiento registrado. El activo quedó En Mantenimiento."
                    : "Mantenimiento registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = FixedAssetsEndpoints.ToMaintenanceResponse(maintenance)
            };

            return Results.Created($"/api/fixed-assets/{id}/maintenances/{maintenance.Id}", response);
        })
        .WithName("CreateFixedAssetMaintenance")
        .Produces<ApiResponse<FixedAssetMaintenanceResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
