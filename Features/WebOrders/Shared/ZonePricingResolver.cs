using Microsoft.EntityFrameworkCore;

public static class ZonePricingResolver
{
    public static async Task<(District? District, ZonePriceList? PriceList)> GetActivePriceListForDistrictAsync(
        RecepcionDbContext db, int districtId)
    {
        var district = await db.Districts
            .Include(x => x.Zone)
            .FirstOrDefaultAsync(x => x.Id == districtId && x.IsActive);

        if (district is null)
        {
            return (null, null);
        }

        var priceList = await db.ZonePriceLists
            .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
            .FirstOrDefaultAsync(x => x.ZoneId == district.ZoneId && x.IsActive);

        return (district, priceList);
    }
}
