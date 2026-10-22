using System.ComponentModel.DataAnnotations;

// Una fila por CLIENTE en cada corrida del job de facturas vencidas (no una fila
// por factura; para eso esta InvoiceReminderLogDetail).
public class InvoiceReminderLog
{
    public long Id { get; set; }

    public int ScheduledEmailNotificationId { get; set; }

    public ScheduledEmailNotification? ScheduledEmailNotification { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime RunAt { get; set; }

    public InvoiceReminderLogStatus Status { get; set; }

    [MaxLength(300)]
    public string? SkipReason { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(1000)]
    public string Recipients { get; set; } = string.Empty;

    public int InvoiceCount { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<InvoiceReminderLogDetail> Details { get; set; } = new List<InvoiceReminderLogDetail>();
}
