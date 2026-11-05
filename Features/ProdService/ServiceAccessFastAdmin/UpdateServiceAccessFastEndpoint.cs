using Microsoft.EntityFrameworkCore;

public static class UpdateServiceAccessFastEndpoint
{
    public static void MapUpdateServiceAccessFast(this IEndpointRouteBuilder app)
    {
        app.MapPut("/access-fast/{id:int}", async (int id, UpdateServiceAccessFastRequest request, RecepcionDbContext db) =>
        {
            var entity = await db.ServiceAccessFasts.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
            {
                return Results.NotFound(new[] { "Acceso rápido no encontrado." });
            }

            if (string.IsNullOrWhiteSpace(request.ShortName))
            {
                return Results.BadRequest(new[] { "El nombre corto es obligatorio." });
            }

            var prodService = await db.ProdServices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProdServiceId);
            if (prodService is null)
            {
                return Results.BadRequest(new[] { "El servicio/producto indicado no existe." });
            }

            entity.ProdServiceID = request.ProdServiceId;
            entity.ShortName = request.ShortName;
            entity.IconName = string.IsNullOrWhiteSpace(request.IconName) ? "bolt" : request.IconName;
            entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status;

            await db.SaveChangesAsync();

            var response = new ServiceAccessFastAdminDto(
                entity.Id, entity.ProdServiceID, prodService.Name, entity.ShortName, entity.IconName, entity.Status, entity.Order);

            return Results.Ok(new ApiResponse<ServiceAccessFastAdminDto>
            {
                Success = true,
                Message = "Acceso rápido actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("UpdateServiceAccessFast")
        .Produces<ApiResponse<ServiceAccessFastAdminDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
