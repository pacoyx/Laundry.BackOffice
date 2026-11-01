using DexterCompany.Models;

public static class CreateCatServiceEndpoints
{
    public static void MapCreateCatService(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (RecepcionDbContext context, CreateCatServiceDto createCatServiceDto) =>
        {
            if (createCatServiceDto == null || string.IsNullOrWhiteSpace(createCatServiceDto.Name))
            {
                return Results.BadRequest(new[] { "El nombre de la categoría es obligatorio." });
            }

            var catService = new CatService()
            {
                Name = createCatServiceDto.Name,
                Description = createCatServiceDto.Description ?? string.Empty,
                Icon = createCatServiceDto.Icon ?? string.Empty
            };

            await context.CatServices.AddAsync(catService);
            await context.SaveChangesAsync();

            var response = new ApiResponse<CatServiceResponseDto>
            {
                Success = true,
                Message = "Categoría de servicio registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = new CatServiceResponseDto(catService.Id, catService.Name, catService.Description, catService.Icon)
            };

            return Results.Created($"/api/catService/{catService.Id}", response);
        });
    }
}
