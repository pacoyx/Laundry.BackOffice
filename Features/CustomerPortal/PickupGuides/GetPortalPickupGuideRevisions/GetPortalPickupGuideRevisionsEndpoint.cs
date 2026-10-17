using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetPortalPickupGuideRevisionsEndpoint
{
    public static void MapGetPortalPickupGuideRevisions(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pickup-guides/{id:long}/revisions", async (long id, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            // Filtrado obligatorio por CustomerId del token: nunca por el request.
            var guideExists = await db.PickupGuides
                .AnyAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (!guideExists)
            {
                return Results.NotFound();
            }

            var revisions = await db.PickupGuideRevisions
                .AsNoTracking()
                .Where(x => x.PickupGuideId == id)
                .Include(x => x.Employee)
                .Include(x => x.DetailRevisions).ThenInclude(d => d.PickupGuideDetail).ThenInclude(d => d.ServiceItem)
                .OrderByDescending(x => x.RevisionDate)
                .ToListAsync();

            var response = revisions.Select(r => new PortalPickupGuideRevisionResponse(
                r.RevisionDate,
                $"{r.Employee.FirstName} {r.Employee.LastName}".Trim(),
                r.Observation,
                r.DetailRevisions.Select(dr => new PortalPickupGuideRevisionItemResponse(
                    dr.ActionType, dr.PickupGuideDetail.ServiceItem.Name, dr.OriginalQuantity, dr.NewQuantity, dr.Reason
                )).ToList()
            )).ToList();

            return Results.Ok(new ApiResponse<List<PortalPickupGuideRevisionResponse>>
            {
                Success = true,
                Message = "Historial de revisiones de la guía.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetPortalPickupGuideRevisions")
        .Produces<ApiResponse<List<PortalPickupGuideRevisionResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GuidesView);
    }
}
