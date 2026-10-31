public record CreateGuiaRemisionRequest(
    string Serie,
    string Numero,
    string DriverName,
    string DriverDni,
    string? DeliveryAddress,
    DateTime FechaTraslado,
    string? MotivoTraslado,
    int? VehicleId,
    string? OriginAddress);
