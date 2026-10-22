using Hangfire;
using Microsoft.EntityFrameworkCore;

// Mantiene sincronizados los recurring jobs de Hangfire con el estado de ScheduledEmailNotifications
// en base de datos. El "Code" de la configuracion se usa como Id del recurring job en Hangfire.
public static class ScheduledEmailNotificationJobScheduler
{
    // Hangfire interpreta el cron en UTC por defecto; sin especificar la zona horaria del
    // negocio aqui, "0 6 * * ..." correria a la 1am hora Peru en vez de las 6am esperadas.
    private static readonly TimeZoneInfo BusinessTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

    public static void SyncJob(this IRecurringJobManager recurringJobManager, ScheduledEmailNotification config)
    {
        if (config.IsEnabled)
        {
            recurringJobManager.AddOrUpdate<ScheduledEmailNotificationJob>(
                config.Code,
                job => job.ExecuteAsync(config.Id, CancellationToken.None),
                config.CronExpression,
                new RecurringJobOptions { TimeZone = BusinessTimeZone });
        }
        else
        {
            recurringJobManager.RemoveIfExists(config.Code);
        }
    }

    public static async Task RegisterScheduledEmailJobsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RecepcionDbContext>();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        var enabledConfigs = await db.ScheduledEmailNotifications
            .AsNoTracking()
            .Where(x => x.IsEnabled)
            .ToListAsync();

        foreach (var config in enabledConfigs)
        {
            recurringJobManager.SyncJob(config);
        }
    }
}
