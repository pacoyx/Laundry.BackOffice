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

            var assetExists = await db.FixedAssets.AnyAsync(x => x.Id == id);
            if (!assetExists)
            {
                return Results.NotFound("Activo fijo no encontrado.");
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
                CreatedAt = DateTime.UtcNow
            };

            await db.FixedAssetMaintenances.AddAsync(maintenance);
            await db.SaveChangesAsync();

            var response = new ApiResponse<FixedAssetMaintenanceResponse>
            {
                Success = true,
                Message = "Mantenimiento registrado correctamente.",
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
