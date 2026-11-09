using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class GetGarmentTicketServiceItemsEndpoint
{
    public static void MapGetGarmentTicketServiceItems(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garment-tickets/service-items", async (ClaimsPrincipal principal, RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            var priceList = await db.CustomerPriceLists
                .AsNoTracking()
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId.Value && x.IsActive);

            // Los servicios tarifados por peso quedan fuera: el ticket de roperia solo registra
            // cantidad de prendas, y la guia de recojo exige el peso para esos servicios al crearse.
            var items = priceList?.Details
                .Where(d => d.UnitPrice > 0 && d.ServiceItem.IsPeso != "S")
                .Select(d => new GarmentTicketServiceItemLookupDto(d.ServiceItemId, d.ServiceItem.Name))
                .OrderBy(x => x.Name)
                .ToList() ?? new List<GarmentTicketServiceItemLookupDto>();

            return Results.Ok(new ApiResponse<List<GarmentTicketServiceItemLookupDto>>
            {
                Success = true,
                Message = items.Count > 0 ? "Servicios disponibles para la empresa." : "La empresa no tiene una tarifa activa.",
                StatusCode = StatusCodes.Status200OK,
                Data = items
            });
        })
        .WithName("GetGarmentTicketServiceItems")
        .Produces<ApiResponse<List<GarmentTicketServiceItemLookupDto>>>(StatusCodes.Status200OK)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}
