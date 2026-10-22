// Una fila por FACTURA incluida en un InvoiceReminderLog. Balance/DaysOverdue son
// una foto del momento del envio (la factura real puede cambiar despues).
public class InvoiceReminderLogDetail
{
    public long Id { get; set; }

    public long InvoiceReminderLogId { get; set; }

    public InvoiceReminderLog? InvoiceReminderLog { get; set; }

    public long InvoiceId { get; set; }

    public Invoice? Invoice { get; set; }

    public decimal Balance { get; set; }

    public int DaysOverdue { get; set; }
}
