using Microsoft.EntityFrameworkCore;

public static class GetPickupGuideRevisionsEndpoint
{
    public static void MapGetPickupGuideRevisions(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}/revisions", async (long id, RecepcionDbContext db) =>
        {
            var guideExists = await db.PickupGuides.AnyAsync(x => x.Id == id);
            if (!guideExists)
            {
                return Results.NotFound("Guía no encontrada.");
            }

            var revisions = await db.PickupGuideRevisions
                .AsNoTracking()
                .AsSplitQuery()
                .Where(x => x.PickupGuideId == id)
                .Include(x => x.Employee)
                .Include(x => x.DetailRevisions).ThenInclude(d => d.PickupGuideDetail).ThenInclude(d => d.ServiceItem)
                .Include(x => x.Photos)
                .OrderByDescending(x => x.RevisionDate)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<PickupGuideRevisionResponse>>
            {
                Success = true,
                Message = "Historial de revisiones de la guía.",
                StatusCode = StatusCodes.Status200OK,
                Data = revisions.Select(PickupGuideRevisionsEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetPickupGuideRevisions")
        .Produces<ApiResponse<List<PickupGuideRevisionResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
