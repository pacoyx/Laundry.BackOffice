using Microsoft.EntityFrameworkCore;

public static class GetPickupGuideEndpoint
{
    public static void MapGetPickupGuide(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}", async (long id, RecepcionDbContext db) =>
        {
            var guide = await db.PickupGuides
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .Include(x => x.Employee)
                .Include(x => x.Vehicle)
                .Include(x => x.Attachments)
                .Include(x => x.Revisions).ThenInclude(r => r.Employee)
                .Include(x => x.Revisions).ThenInclude(r => r.DetailRevisions).ThenInclude(dr => dr.PickupGuideDetail).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (guide is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new ApiResponse<PickupGuideResponse>
            {
                Success = true,
                Message = "Guía de recojo obtenida correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = PickupGuidesEndpoints.ToResponse(guide)
            });
        })
        .WithName("GetPickupGuide")
        .Produces<ApiResponse<PickupGuideResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
