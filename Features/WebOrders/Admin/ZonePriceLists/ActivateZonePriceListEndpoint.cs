using Microsoft.EntityFrameworkCore;

public static class ActivateZonePriceListEndpoint
{
    public static void MapActivateZonePriceList(this IEndpointRouteBuilder app)
    {
        app.MapPost("/zone-price-lists/{id:long}/activate", async (long id, RecepcionDbContext db) =>
        {
            var priceList = await db.ZonePriceLists.FirstOrDefaultAsync(x => x.Id == id);
            if (priceList is null)
            {
                return Results.NotFound("Tarifa no encontrada.");
            }

            if (!priceList.IsActive)
            {
                var currentlyActive = await db.ZonePriceLists
                    .Where(x => x.ZoneId == priceList.ZoneId && x.IsActive)
                    .ToListAsync();
                foreach (var active in currentlyActive)
                {
                    active.IsActive = false;
                }

                priceList.IsActive = true;
                await db.SaveChangesAsync();
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Tarifa activada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { priceList.Id, priceList.IsActive }
            });
        })
        .WithName("ActivateZonePriceList")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}
