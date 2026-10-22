using System.Globalization;
using Microsoft.EntityFrameworkCore;

public class ScheduledEmailService : IScheduledEmailService
{
    private readonly RecepcionDbContext _db;
    private readonly IEmailService _emailService;

    public ScheduledEmailService(RecepcionDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<ScheduledEmailExecution> ExecuteAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default)
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

        var config = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .Include(x => x.Recipients)
            .FirstOrDefaultAsync(x => x.Id == scheduledEmailNotificationId, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException($"Configuración de correo programado {scheduledEmailNotificationId} no encontrada.");
        }

        var toRecipients = config.Recipients
            .Where(r => r.RecipientType == ScheduledEmailRecipientType.To)
            .Select(r => r.Email)
            .ToList();
        var bccRecipients = config.Recipients
            .Where(r => r.RecipientType == ScheduledEmailRecipientType.Bcc)
            .Select(r => r.Email)
            .ToList();
        var recipientsSnapshot = string.Join(", ", toRecipients.Concat(bccRecipients));

        ScheduledEmailExecutionStatus status;
        string? errorMessage = null;

        if (toRecipients.Count == 0)
        {
            status = ScheduledEmailExecutionStatus.Failed;
            errorMessage = "No hay destinatarios TO configurados.";
        }
        else
        {
            try
            {
                var subject = ResolvePlaceholders(config.SubjectTemplate, config, now);
                var body = ResolvePlaceholders(config.BodyTemplate, config, now);

                var message = new EmailMessage(toRecipients, subject, body, Array.Empty<EmailAttachment>(), bccRecipients);
                await _emailService.SendAsync(message, EmailOperationCodes.AutomatedTasks, cancellationToken);

                status = ScheduledEmailExecutionStatus.Success;
            }
            catch (Exception ex)
            {
                status = ScheduledEmailExecutionStatus.Failed;
                errorMessage = ex.Message;
            }
        }

        var execution = new ScheduledEmailExecution
        {
            ScheduledEmailNotificationId = scheduledEmailNotificationId,
            ExecutedAt = now,
            Status = status,
            ErrorMessage = errorMessage,
            Recipients = recipientsSnapshot,
            CreatedAt = now
        };

        _db.ScheduledEmailExecutions.Add(execution);
        await _db.SaveChangesAsync(cancellationToken);

        return execution;
    }

    public async Task SendTestAsync(int scheduledEmailNotificationId, string testEmail, CancellationToken cancellationToken = default)
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

        var config = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == scheduledEmailNotificationId, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException($"Configuración de correo programado {scheduledEmailNotificationId} no encontrada.");
        }

        var subject = $"[PRUEBA] {ResolvePlaceholders(config.SubjectTemplate, config, now)}";
        var body = ResolvePlaceholders(config.BodyTemplate, config, now);

        var message = new EmailMessage(new[] { testEmail }, subject, body, Array.Empty<EmailAttachment>());
        await _emailService.SendAsync(message, EmailOperationCodes.AutomatedTasks, cancellationToken);
    }

    public async Task<(string Subject, string Body)> PreviewAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default)
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

        var config = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == scheduledEmailNotificationId, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException($"Configuración de correo programado {scheduledEmailNotificationId} no encontrada.");
        }

        var subject = ResolvePlaceholders(config.SubjectTemplate, config, now);
        var body = ResolvePlaceholders(config.BodyTemplate, config, now);

        return (subject, body);
    }

    private static string ResolvePlaceholders(string template, ScheduledEmailNotification config, DateTime now)
    {
        var culture = new CultureInfo("es-PE");
        var dayName = culture.TextInfo.ToTitleCase(now.ToString("dddd", culture));

        return template
            .Replace("{Date}", now.ToString("dd/MM/yyyy"))
            .Replace("{DayName}", dayName)
            .Replace("{DriverName}", config.DriverName)
            .Replace("{DriverDocument}", config.DriverDocument)
            .Replace("{SignatureName}", config.SignatureName)
            .Replace("{SignaturePosition}", config.SignaturePosition)
            .Replace("{SignaturePhone}", config.SignaturePhone);
    }
}
