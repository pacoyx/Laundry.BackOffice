using Microsoft.EntityFrameworkCore;

public class OverdueInvoiceReminderJob
{
    private readonly RecepcionDbContext _db;
    private readonly IOverdueInvoiceReminderService _service;
    private readonly IAppLogger<OverdueInvoiceReminderJob> _logger;

    public OverdueInvoiceReminderJob(RecepcionDbContext db, IOverdueInvoiceReminderService service, IAppLogger<OverdueInvoiceReminderJob> logger)
    {
        _db = db;
        _service = service;
        _logger = logger;
    }

    public async Task ExecuteAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default)
    {
        var isEnabled = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .Where(x => x.Id == scheduledEmailNotificationId)
            .Select(x => x.IsEnabled)
            .FirstOrDefaultAsync(cancellationToken);

        if (!isEnabled)
        {
            _logger.LogInformacion("OverdueInvoiceReminder {0} esta deshabilitada, se omite la ejecucion.", scheduledEmailNotificationId);
            return;
        }

        await _service.ExecuteAsync(scheduledEmailNotificationId, cancellationToken);
    }
}
