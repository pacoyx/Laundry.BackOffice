using DexterCompany.Models;

public record ResponseWgmDto
(
     string SerieGuia,
     string NumeroGuia,
     DateTime FechaOperacion,
     DateTime FechaHoraEntrega,
     string MensajeAlertas,
     string Observaciones,
     string TipoPago,
     string DescripcionPago,
     string TipoRecepcion,
     string DireccionContacto,
     string TelefonoContacto,
     decimal Total,
     decimal Acuenta,
     decimal Saldo,
     int CustomerId,
     string CustomerName,
     string CustomerPhone,
     string EstadoPago,
     DateTime? fechaPago,
     string estadoRegistro,
     string estadoSituacion,
     DateTime? fechaRecojo,
     IEnumerable<ResponseWgdDto> WorkGuideDetailsDTO
);


public record ResponseWgdDto(
    int Id,
    decimal Cant,
    decimal Precio,
    decimal Total,
    string Observaciones,
    string TipoLavado,
    string Ubicacion,
    string EstadoTrabajo,
    int ProductId,
    ProdService Product,
    string EstadoRegistro,
    string EstadoSituacion,
    string EstadoPago,
    DateTime? FechaEnvioUbicacion,
    DateTime? FechaEntregaAproxUbicacion
);