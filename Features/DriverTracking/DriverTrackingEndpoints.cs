namespace _24dex_backend_comercial.Features.DriverTracking;

// API consumida por el backoffice para pintar el mapa de ubicacion de choferes. La ubicacion en
// si la mandan los choferes vía /api/pickup-mobile/location (ver SaveDriverLocationEndpoint) -este
// grupo es solo de lectura, para quien esta viendo el mapa.
public static class DriverTrackingEndpoints
{
    public static void MapDriverTracking(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/driver-tracking")
            .WithTags("DriverTracking")
            .RequirePermission(PermissionCodes.DriverTracking);

        group.MapGetDriverLocations();
        group.MapGetDriverTrackingDrivers();
        group.MapGetDriverRoute();
    }
}
