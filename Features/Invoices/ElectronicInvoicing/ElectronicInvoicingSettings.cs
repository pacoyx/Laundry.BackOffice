public class ElectronicInvoicingSettings
{
    // Switch de encendido/apagado: mientras Provider este vacio (o no sea "NubeFact"),
    // no se envia nada y ni siquiera se encola el job en Hangfire. Para activarlo de
    // verdad, poner Provider = "NubeFact" y completar la seccion NubeFact.
    public string Provider { get; set; } = string.Empty;

    public NubeFactSettings NubeFact { get; set; } = new();
}

public class NubeFactSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}
