using Microsoft.EntityFrameworkCore;

public static class DeleteCatServiceEndpoint
{
    public static void MapDeleteCat(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (RecepcionDbContext context, int id) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest("Id must be greater than zero");
            }

            var catService = await context.CatServices.FirstOrDefaultAsync(x => x.Id == id);
            if (catService == null)
            {
                return Results.NotFound();
            }

            // ProdService.CatServiceId no tiene FK real a nivel de base de datos (solo es un int),
            // asi que borrar una categoria en uso no fallaria con un error de SQL: dejaria a esos
            // servicios con una categoria inexistente, invisibles para el join de busqueda. Se
            // bloquea explicitamente en vez de permitir ese estado inconsistente.
            var enUso = await context.ProdServices.AnyAsync(x => x.CatServiceId == id);
            if (enUso)
            {
                return Results.BadRequest(new[] { "No se puede eliminar: hay servicios registrados con esta categoría." });
            }

            context.CatServices.Remove(catService);
            await context.SaveChangesAsync();

            var response = new ApiResponse<string>
            {
                Success = true,
                Message = "Categoría de servicio eliminada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = "Categoría de servicio eliminada"
            };

            return Results.Ok(response);
        });
    }
}
