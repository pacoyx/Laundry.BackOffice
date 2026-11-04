namespace _24dex_backend_comercial.Features.DriverTracking;

// Se usa tanto para la respuesta de GetDriverLocations (snapshot inicial del mapa) como para el
// payload que se transmite por DriverTrackingHub ("DriverLocationUpdated") en cada ping nuevo.
public record DriverLocationDto(
    long EmployeeId,
    string EmployeeName,
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    DateTime RecordedAt
);
