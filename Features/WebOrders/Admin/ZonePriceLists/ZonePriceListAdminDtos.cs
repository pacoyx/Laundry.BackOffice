public record ZonePriceListDetailRequest(int ServiceItemId, decimal UnitPrice);

public record ZonePriceListDetailAdminResponse(long Id, int ServiceItemId, string ServiceItemName, decimal UnitPrice);

public record ZonePriceListAdminResponse(
    long Id,
    int ZoneId,
    string ZoneName,
    string Name,
    int Version,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive,
    List<ZonePriceListDetailAdminResponse> Details);

public record ZonePriceListAdminSummaryResponse(
    long Id,
    int ZoneId,
    string ZoneName,
    string Name,
    int Version,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive);

public record CreateZonePriceListRequest(
    string Name,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool Activate,
    List<ZonePriceListDetailRequest> Details);

public record UpdateZonePriceListRequest(
    string Name,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    List<ZonePriceListDetailRequest> Details);
