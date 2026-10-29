public static class ZonePriceListsAdminMapper
{
    public static ZonePriceListAdminResponse ToResponse(ZonePriceList priceList) => new(
        priceList.Id,
        priceList.ZoneId,
        priceList.Zone.Name,
        priceList.Name,
        priceList.Version,
        priceList.EffectiveFrom,
        priceList.EffectiveTo,
        priceList.IsActive,
        priceList.Details.Select(d => new ZonePriceListDetailAdminResponse(
            d.Id, d.ServiceItemId, d.ServiceItem.Name, d.UnitPrice
        )).ToList()
    );

    public static ZonePriceListAdminSummaryResponse ToSummaryResponse(ZonePriceList priceList) => new(
        priceList.Id,
        priceList.ZoneId,
        priceList.Zone.Name,
        priceList.Name,
        priceList.Version,
        priceList.EffectiveFrom,
        priceList.EffectiveTo,
        priceList.IsActive
    );
}
