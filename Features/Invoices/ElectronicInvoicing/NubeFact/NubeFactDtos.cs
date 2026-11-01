// Portado del ejemplo oficial de NubeFact (www.nubefact.com/integracion). La API de NubeFact
// es quisquillosa con varios campos "opcionales": si no aplican, espera literalmente el string
// vacio "" en el JSON, no el numero 0 ni un null. Por eso varios campos son "object" en vez de
// double/int: al mapear, asignar "" (string) cuando no aplique, nunca dejar en null.
//
// Los nombres de propiedad se serializan a snake_case (ej. TipoDeComprobante -> tipo_de_comprobante)
// via JsonNamingPolicy.SnakeCaseLower configurado en NubeFactElectronicInvoicingService, por eso
// no llevan [JsonPropertyName] individual.
public class NubeFactInvoiceRequest
{
    public string Operacion { get; set; } = "generar_comprobante";
    public int TipoDeComprobante { get; set; }
    public string Serie { get; set; } = string.Empty;
    public int Numero { get; set; }
    public int SunatTransaction { get; set; } = 1;
    public int ClienteTipoDeDocumento { get; set; }
    public string ClienteNumeroDeDocumento { get; set; } = string.Empty;
    public string ClienteDenominacion { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public DateTime FechaDeEmision { get; set; }
    public DateTime FechaDeVencimiento { get; set; }
    public int Moneda { get; set; } = 1;
    public object TipoDeCambio { get; set; } = "";
    public double PorcentajeDeIgv { get; set; }
    public object DescuentoGlobal { get; set; } = "";
    public object TotalDescuento { get; set; } = "";
    public object TotalAnticipo { get; set; } = "";
    public double TotalGravada { get; set; }
    public object TotalInafecta { get; set; } = "";
    public object TotalExonerada { get; set; } = "";
    public double TotalIgv { get; set; }
    public object TotalGratuita { get; set; } = "";
    public object TotalOtrosCargos { get; set; } = "";
    public double Total { get; set; }
    public bool Detraccion { get; set; } = false;
    public string Observaciones { get; set; } = string.Empty;
    public bool EnviarAutomaticamenteALaSunat { get; set; } = true;
    public bool EnviarAutomaticamenteAlCliente { get; set; } = false;
    public List<NubeFactItem> Items { get; set; } = new();
}

public class NubeFactItem
{
    public string UnidadDeMedida { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public double Cantidad { get; set; }
    public double ValorUnitario { get; set; }
    public double PrecioUnitario { get; set; }
    public object Descuento { get; set; } = "";
    public double Subtotal { get; set; }
    public int TipoDeIgv { get; set; } = 1;
    public double Igv { get; set; }
    public double Total { get; set; }
}

public class NubeFactResponse
{
    public string? Errors { get; set; }
    public int Tipo { get; set; }
    public string? Serie { get; set; }
    public int Numero { get; set; }
    public string? Url { get; set; }
    public bool? AceptadaPorSunat { get; set; }
    public string? SunatDescription { get; set; }
    public string? SunatResponseCode { get; set; }
}
