using Microsoft.EntityFrameworkCore;

public static class DeleteServiceAccessFastEndpoint
{
    public static void MapDeleteServiceAccessFast(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/access-fast/{id:int}", async (int id, RecepcionDbContext db) =>
        {
            var entity = await db.ServiceAccessFasts.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
            {
                return Results.NotFound(new[] { "Acceso rápido no encontrado." });
            }

            db.ServiceAccessFasts.Remove(entity);
            await db.SaveChangesAsync();

            // Recompacta el orden de los que quedan (1..N sin huecos) para que la lista
            // siga siendo consistente despues de borrar uno del medio.
            var remaining = await db.ServiceAccessFasts.OrderBy(x => x.Order).ToListAsync();
            for (var i = 0; i < remaining.Count; i++)
            {
                remaining[i].Order = i + 1;
            }
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Acceso rápido eliminado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = "OK"
            });
        })
        .WithName("DeleteServiceAccessFast")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
