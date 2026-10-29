using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateZonePriceListEndpoint
{
    public static void MapCreateZonePriceList(this IEndpointRouteBuilder app)
    {
        app.MapPost("/zones/{zoneId:int}/price-lists", async (
            int zoneId,
            CreateZonePriceListRequest request,
            IValidator<CreateZonePriceListRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var zone = await db.Zones.FirstOrDefaultAsync(x => x.Id == zoneId);
            if (zone is null)
            {
                return Results.NotFound("Zona no encontrada.");
            }

            var serviceItemIds = request.Details.Select(d => d.ServiceItemId).ToList();
            var existingServiceItems = await db.ProdServices
                .Where(x => serviceItemIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            var missingServiceItems = serviceItemIds.Except(existingServiceItems).ToList();
            if (missingServiceItems.Count > 0)
            {
                return Results.BadRequest(new[] { $"Los siguientes servicios no existen: {string.Join(", ", missingServiceItems)}" });
            }

            var nextVersion = await db.ZonePriceLists
                .Where(x => x.ZoneId == zoneId)
                .Select(x => (int?)x.Version)
                .MaxAsync() ?? 0;
            nextVersion++;

            if (request.Activate)
            {
                var currentlyActive = await db.ZonePriceLists
                    .Where(x => x.ZoneId == zoneId && x.IsActive)
                    .ToListAsync();
                foreach (var active in currentlyActive)
                {
                    active.IsActive = false;
                }
            }

            var priceList = new ZonePriceList
            {
                ZoneId = zoneId,
                Name = request.Name,
                Version = nextVersion,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsActive = request.Activate,
                CreatedAt = DateTime.UtcNow,
                Details = request.Details.Select(d => new ZonePriceListDetail
                {
                    ServiceItemId = d.ServiceItemId,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };

            db.ZonePriceLists.Add(priceList);
            await db.SaveChangesAsync();

            await db.Entry(priceList).Collection(x => x.Details).Query()
                .Include(x => x.ServiceItem)
                .LoadAsync();
            await db.Entry(priceList).Reference(x => x.Zone).LoadAsync();

            return Results.Created($"/api/web-orders/admin/zone-price-lists/{priceList.Id}", new ApiResponse<ZonePriceListAdminResponse>
            {
                Success = true,
                Message = "Tarifa de zona registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = ZonePriceListsAdminMapper.ToResponse(priceList)
            });
        })
        .WithName("CreateZonePriceList")
        .Produces<ApiResponse<ZonePriceListAdminResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionMaestros);
    }
}
