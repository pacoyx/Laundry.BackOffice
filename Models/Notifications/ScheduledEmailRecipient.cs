using System.ComponentModel.DataAnnotations;

public class ScheduledEmailRecipient
{
    public int Id { get; set; }

    public int ScheduledEmailNotificationId { get; set; }

    public ScheduledEmailNotification? ScheduledEmailNotification { get; set; }

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public ScheduledEmailRecipientType RecipientType { get; set; }

    public DateTime CreatedAt { get; set; }
}
