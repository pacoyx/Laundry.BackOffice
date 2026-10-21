using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class GetPortalPickupGuidesEndpoint
{
    public static void MapGetPortalPickupGuides(this IEndpointRouteBuilder app)
    {
        app.MapGet("/pickup-guides", async (
            ClaimsPrincipal principal,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] PickupGuideStatus? status,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var query = db.PickupGuides
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId.Value)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.PickupDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.PickupDate.Date <= toDate.Value.Date);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var response = await query
                .OrderByDescending(x => x.PickupDate)
                .Select(g => new PortalPickupGuideSummaryResponse(
                    g.Id, g.GuideNumber, g.PhysicalGuideNumber, g.BranchSalesId, g.PickupDate, g.Status, g.TotalAmount, g.CreatedAt,
                    g.Attachments.Any(a => a.IsCurrent && a.AttachmentType == PickupGuideAttachmentType.Pdf),
                    g.Attachments.Any(a => a.IsCurrent && a.AttachmentType == PickupGuideAttachmentType.Photo),
                    g.Attachments.Any(a => a.IsCurrent && a.AttachmentType == PickupGuideAttachmentType.Signature),
                    g.Revisions.Any()
                ))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<PortalPickupGuideSummaryResponse>>
            {
                Success = true,
                Message = "Listado de guías de recojo de la empresa.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetPortalPickupGuides")
        .Produces<ApiResponse<List<PortalPickupGuideSummaryResponse>>>(StatusCodes.Status200OK)
        .RequireCustomerPermission(CustomerPermissionCodes.GuidesView);
    }
}
