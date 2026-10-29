public record CreateDistrictRequest(string Name, int ZoneId);

public record UpdateDistrictRequest(string Name, int ZoneId, bool IsActive);

public record DistrictAdminResponse(int Id, string Name, int ZoneId, string ZoneName, bool IsActive);
