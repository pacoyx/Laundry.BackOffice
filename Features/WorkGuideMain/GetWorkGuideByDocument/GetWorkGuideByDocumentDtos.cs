using DexterCompany.Models;

public record ResponseGuiaByDocumentDto
(
    int Id,
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
     string TipoPagoCancelacion,
     IEnumerable<ResponseGuiaByDocumentDtoDet> WorkGuideDetailsDTO
);


public record ResponseGuiaByDocumentDtoDet(
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
    DateTime? FechaRecojo ,
    DateTime? FechaDevolucion ,
    string Identificador,
    DateTime? FechaEnvioUbicacion,
    DateTime? FechaEntregaAproxUbicacion
);