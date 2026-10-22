using System.ComponentModel.DataAnnotations;

public class ScheduledEmailExecution
{
    public long Id { get; set; }

    public int ScheduledEmailNotificationId { get; set; }

    public ScheduledEmailNotification? ScheduledEmailNotification { get; set; }

    public DateTime ExecutedAt { get; set; }

    public ScheduledEmailExecutionStatus Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    // Snapshot de los correos a los que se intento enviar en esta ejecucion (separados por coma),
    // independiente de la configuracion de destinatarios vigente (que puede cambiar despues).
    [MaxLength(1000)]
    public string Recipients { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
