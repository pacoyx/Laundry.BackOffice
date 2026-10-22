using System.ComponentModel.DataAnnotations;

public class ScheduledEmailNotification
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CronExpression { get; set; } = string.Empty;

    [MaxLength(300)]
    public string SubjectTemplate { get; set; } = string.Empty;

    public string BodyTemplate { get; set; } = string.Empty;

    // Solo aplica a configuraciones tipo chofer; otros casos (ej. cobranza) lo dejan vacio.
    [MaxLength(150)]
    public string? DriverName { get; set; }

    [MaxLength(20)]
    public string? DriverDocument { get; set; }

    [MaxLength(150)]
    public string SignatureName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SignaturePosition { get; set; } = string.Empty;

    [MaxLength(30)]
    public string SignaturePhone { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    // Fixed: usa ScheduledEmailRecipients. Dynamic: el job resuelve destinatarios
    // por su cuenta (ej. facturas vencidas, uno por cliente). No editable tras crear.
    public ScheduledEmailRecipientMode RecipientMode { get; set; } = ScheduledEmailRecipientMode.Fixed;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public ICollection<ScheduledEmailRecipient> Recipients { get; set; } = new List<ScheduledEmailRecipient>();

    public ICollection<ScheduledEmailExecution> Executions { get; set; } = new List<ScheduledEmailExecution>();
}
