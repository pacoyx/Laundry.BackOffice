using Microsoft.EntityFrameworkCore;

// El frontend manda la lista COMPLETA de Ids en el nuevo orden deseado (ej. tras un
// drag&drop); el Order de cada uno se recalcula como su posicion en esa lista, asi que
// mover un elemento automaticamente corre a todos los demas sin pedirle al cliente que
// calcule gaps/swaps.
public static class ReorderServiceAccessFastEndpoint
{
    public static void MapReorderServiceAccessFast(this IEndpointRouteBuilder app)
    {
        app.MapPut("/access-fast/reorder", async (ReorderServiceAccessFastRequest request, RecepcionDbContext db) =>
        {
            if (request.OrderedIds is null || request.OrderedIds.Count == 0)
            {
                return Results.BadRequest(new[] { "Debe indicar el orden de al menos un elemento." });
            }

            var entities = await db.ServiceAccessFasts
                .Where(x => request.OrderedIds.Contains(x.Id))
                .ToListAsync();

            var entityById = entities.ToDictionary(x => x.Id);

            for (var i = 0; i < request.OrderedIds.Count; i++)
            {
                if (entityById.TryGetValue(request.OrderedIds[i], out var entity))
                {
                    entity.Order = i + 1;
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Orden actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = "OK"
            });
        })
        .WithName("ReorderServiceAccessFast")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
