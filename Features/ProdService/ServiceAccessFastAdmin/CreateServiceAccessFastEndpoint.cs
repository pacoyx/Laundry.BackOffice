using Microsoft.EntityFrameworkCore;

public static class CreateServiceAccessFastEndpoint
{
    public static void MapCreateServiceAccessFast(this IEndpointRouteBuilder app)
    {
        app.MapPost("/access-fast", async (CreateServiceAccessFastRequest request, RecepcionDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.ShortName))
            {
                return Results.BadRequest(new[] { "El nombre corto es obligatorio." });
            }

            var prodService = await db.ProdServices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ProdServiceId);
            if (prodService is null)
            {
                return Results.BadRequest(new[] { "El servicio/producto indicado no existe." });
            }

            // Se agrega siempre al final de la lista; el orden especifico se ajusta despues
            // arrastrando en la pantalla de administracion (ver ReorderServiceAccessFastEndpoint).
            var maxOrder = await db.ServiceAccessFasts.Select(x => (int?)x.Order).MaxAsync() ?? 0;

            var entity = new ServiceAccessFast
            {
                ProdServiceID = request.ProdServiceId,
                ShortName = request.ShortName,
                IconName = string.IsNullOrWhiteSpace(request.IconName) ? "bolt" : request.IconName,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "A" : request.Status,
                Order = maxOrder + 1
            };

            db.ServiceAccessFasts.Add(entity);
            await db.SaveChangesAsync();

            var response = new ServiceAccessFastAdminDto(
                entity.Id, entity.ProdServiceID, prodService.Name, entity.ShortName, entity.IconName, entity.Status, entity.Order);

            return Results.Ok(new ApiResponse<ServiceAccessFastAdminDto>
            {
                Success = true,
                Message = "Acceso rápido creado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("CreateServiceAccessFast")
        .Produces<ApiResponse<ServiceAccessFastAdminDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
