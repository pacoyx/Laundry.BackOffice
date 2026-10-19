using Microsoft.EntityFrameworkCore;

public static class AssignWorkGuideDetailLocationEndpoint
{
    public static void MapAssignWorkGuideDetailLocation(this IEndpointRouteBuilder app)
    {
        app.MapPost("/workguide-details/{workGuideDetailId:int}/assign", async (
            int workGuideDetailId,
            AssignWorkGuideDetailLocationRequestDto request,
            RecepcionDbContext db,
            CancellationToken cancellationToken) =>
        {
            var detail = await db.WorkGuideDetails
                .Include(d => d.WorkGuideMain)
                .FirstOrDefaultAsync(d => d.Id == workGuideDetailId, cancellationToken);

            if (detail is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Detalle de guía con ID {workGuideDetailId} no encontrado."
                });
            }

            var locationClothes = await db.LocationClothes.FirstOrDefaultAsync(x => x.Id == request.LocationClothesId, cancellationToken);
            if (locationClothes is null)
            {
                return Results.BadRequest(new[] { $"Ubicación con ID {request.LocationClothesId} no encontrada." });
            }

            detail.Ubicacion = locationClothes.Name;

            var location = new LocationWorkGuide
            {
                LocationClothesId = locationClothes.Id,
                WorkGuideId = detail.WorkGuideMainId,
                WorkGuideDetailId = detail.Id,
                Comments = request.Comments,
                NumeroGuia = $"{detail.WorkGuideMain?.NumeroGuia}{detail.Identificador}",
                IsSystem = true,
                FechaEntregaAprox = request.FechaEntregaAprox,
                CreatedDate = DateTime.Now
            };

            await db.LocationWorkGuides.AddAsync(location, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var response = new ApiResponse<WorkGuideDetailLocationResponseDto>
            {
                Success = true,
                Message = "Ubicación actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new WorkGuideDetailLocationResponseDto(
                    detail.Id,
                    detail.Ubicacion,
                    locationClothes.Id,
                    locationClothes.Name,
                    location.CreatedDate,
                    location.FechaEntregaAprox,
                    location.Comments)
            };

            return Results.Ok(response);
        })
        .WithName("AssignWorkGuideDetailLocation")
        .Produces<ApiResponse<WorkGuideDetailLocationResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }
}
