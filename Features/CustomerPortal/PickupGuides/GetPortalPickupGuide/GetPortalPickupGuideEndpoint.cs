using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetPortalPickupGuideEndpoint
{
    public static void MapGetPortalPickupGuide(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pickup-guides/{id:long}", async (long id, ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            // Filtrado obligatorio por CustomerId del token: nunca por el request.
            var guide = await db.PickupGuides
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .Include(x => x.Employee)
                .Include(x => x.Vehicle)
                .Include(x => x.Revisions).ThenInclude(r => r.DetailRevisions)
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (guide is null)
            {
                return Results.NotFound();
            }

            var earliestDetailRevisions = guide.Revisions
                .OrderBy(r => r.RevisionDate)
                .SelectMany(r => r.DetailRevisions)
                .GroupBy(dr => dr.PickupGuideDetailId)
                .ToDictionary(g => g.Key, g => g.First());

            var response = new PortalPickupGuideResponse(
                guide.Id, guide.GuideNumber, guide.PhysicalGuideNumber, guide.BranchSalesId, guide.PickupDate, guide.Status,
                guide.Observation, guide.TotalAmount, guide.CreatedAt,
                guide.Details.Select(d =>
                {
                    var isPricedByWeight = d.ServiceItem.IsPeso == "S";
                    var confirmedQuantity = isPricedByWeight ? d.Weight ?? 0 : d.Quantity;
                    var originalQuantity = earliestDetailRevisions.TryGetValue(d.Id, out var firstRevision)
                        ? firstRevision.OriginalQuantity
                        : confirmedQuantity;

                    return new PortalPickupGuideDetailResponse(
                        d.Id, d.ServiceItemId, d.ServiceItem.Name, d.Quantity, d.Weight, d.UnitPrice, d.Total, d.Observation,
                        originalQuantity, confirmedQuantity
                    );
                }).ToList(),
                guide.Employee is null ? null : new PortalEmployeeSummaryResponse(
                    guide.Employee.Id, $"{guide.Employee.FirstName} {guide.Employee.LastName}".Trim()),
                guide.Vehicle is null ? null : new PortalVehicleSummaryResponse(
                    guide.Vehicle.Id, guide.Vehicle.Plate, guide.Vehicle.Description)
            );

            return Results.Ok(new ApiResponse<PortalPickupGuideResponse>
            {
                Success = true,
                Message = "Guía de recojo obtenida correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetPortalPickupGuide")
        .Produces<ApiResponse<PortalPickupGuideResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireCustomerPermission(CustomerPermissionCodes.GuidesView);
    }
}
