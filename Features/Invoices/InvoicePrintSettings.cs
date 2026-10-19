public class InvoicePrintSettings
{
    public string PaymentAccountNote { get; set; } = string.Empty;

    public InvoiceDetractionSettings Detraction { get; set; } = new();
}

public class InvoiceDetractionSettings
{
    public decimal MinimumAmount { get; set; } = 700m;

    public decimal Percentage { get; set; } = 12m;

    public string Legend { get; set; } = string.Empty;

    public string BienServicioCode { get; set; } = string.Empty;

    public string BienServicioDescription { get; set; } = string.Empty;

    public string MedioPagoCode { get; set; } = string.Empty;

    public string MedioPagoDescription { get; set; } = string.Empty;

    public string BankAccountNumber { get; set; } = string.Empty;
}
