public record ZonePriceListItemResponse(int ServiceItemId, string Name, string Description, decimal UnitPrice);

public record ActiveZonePriceListResponse(
    long ZonePriceListId,
    int ZoneId,
    string ZoneName,
    List<ZonePriceListItemResponse> Items);
