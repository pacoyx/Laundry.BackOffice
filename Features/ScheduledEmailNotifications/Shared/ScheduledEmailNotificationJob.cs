using Microsoft.EntityFrameworkCore;

public class ScheduledEmailNotificationJob
{
    private readonly RecepcionDbContext _db;
    private readonly IScheduledEmailService _scheduledEmailService;
    private readonly IAppLogger<ScheduledEmailNotificationJob> _logger;

    public ScheduledEmailNotificationJob(RecepcionDbContext db, IScheduledEmailService scheduledEmailService, IAppLogger<ScheduledEmailNotificationJob> logger)
    {
        _db = db;
        _scheduledEmailService = scheduledEmailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default)
    {
        // Revalida IsEnabled al momento de ejecutar: el recurring job pudo haber quedado
        // registrado en Hangfire y luego la configuracion se desactivo entre medio.
        var isEnabled = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .Where(x => x.Id == scheduledEmailNotificationId)
            .Select(x => x.IsEnabled)
            .FirstOrDefaultAsync(cancellationToken);

        if (!isEnabled)
        {
            _logger.LogInformacion("ScheduledEmailNotification {0} esta deshabilitada, se omite la ejecucion.", scheduledEmailNotificationId);
            return;
        }

        await _scheduledEmailService.ExecuteAsync(scheduledEmailNotificationId, cancellationToken);
    }
}
