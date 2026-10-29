using Microsoft.EntityFrameworkCore;

public static class DeactivateZonePriceListEndpoint
{
    public static void MapDeactivateZonePriceList(this IEndpointRouteBuilder app)
    {
        app.MapPost("/zone-price-lists/{id:long}/deactivate", async (long id, RecepcionDbContext db) =>
        {
            var priceList = await db.ZonePriceLists.FirstOrDefaultAsync(x => x.Id == id);
            if (priceList is null)
            {
                return Results.NotFound("Tarifa no encontrada.");
            }

            priceList.IsActive = false;
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Tarifa desactivada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { priceList.Id, priceList.IsActive }
            });
        })
        .WithName("DeactivateZonePriceList")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}
