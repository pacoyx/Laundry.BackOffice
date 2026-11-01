using Microsoft.EntityFrameworkCore;

public static class UpdateCatServiceEndpoint
{
    public static void MapUpdateCatService(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (RecepcionDbContext context, int id, UpdateCatServiceDto request) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest("Id must be greater than zero");
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new[] { "El nombre de la categoría es obligatorio." });
            }

            var catService = await context.CatServices.FirstOrDefaultAsync(x => x.Id == id);
            if (catService == null)
            {
                return Results.NotFound();
            }

            catService.Name = request.Name;
            catService.Description = request.Description ?? string.Empty;
            catService.Icon = request.Icon ?? string.Empty;

            await context.SaveChangesAsync();

            var response = new ApiResponse<CatServiceResponseDto>
            {
                Success = true,
                Message = "Categoría de servicio actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new CatServiceResponseDto(catService.Id, catService.Name, catService.Description, catService.Icon)
            };

            return Results.Ok(response);
        });
    }
}
