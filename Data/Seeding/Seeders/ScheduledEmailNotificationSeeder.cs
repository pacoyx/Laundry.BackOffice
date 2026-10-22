using Microsoft.EntityFrameworkCore;

// Primera configuracion del modulo de correos programados: aviso de chofer de recojo.
// Queda IsEnabled = true por pedido explicito, pero sin destinatarios TO todavia (el admin
// debe agregarlos via POST .../recipients); hasta entonces cada ejecucion quedara registrada
// como Failed ("No hay destinatarios TO configurados") sin romper nada.
public class ScheduledEmailNotificationSeeder : IApplicationSeeder
{
    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var exists = await context.ScheduledEmailNotifications
            .AnyAsync(x => x.Code == "PICKUP_DRIVER_NOTICE", cancellationToken);

        if (exists)
        {
            return;
        }

        var bodyTemplate = string.Join("<br/>", new[]
        {
            "ESTIMADOS, BUEN DÍA.",
            "",
            "HOY {DayName} {Date},",
            "SE APERSONARÁ EL SR.",
            "{DriverName},",
            "DNI {DriverDocument}",
            "",
            "PARA ENTREGAR PRENDAS LIMPIAS -",
            "SERVICIO DE LAVANDERÍA KYO -",
            "EMPRESA DEXTER LAUNDRY SERVICE SAC.",
            "",
            "AGRADECEMOS BRINDAR LAS FACILIDADES",
            "CORRESPONDIENTES A NUESTRO PERSONAL.",
            "",
            "ATENTAMENTE",
            "",
            "{SignatureName}",
            "{SignaturePosition}",
            "{SignaturePhone}"
        });

        context.ScheduledEmailNotifications.Add(new ScheduledEmailNotification
        {
            Code = "PICKUP_DRIVER_NOTICE",
            Name = "Aviso Chofer Recojo",
            CronExpression = CronScheduleHelper.BuildCronExpression(6, 0, new[]
            {
                ScheduledWeekday.Monday, ScheduledWeekday.Wednesday, ScheduledWeekday.Thursday, ScheduledWeekday.Saturday
            }),
            SubjectTemplate = "Aviso de Recojo Programado - {Date}",
            BodyTemplate = bodyTemplate,
            DriverName = "Juan Pérez",
            DriverDocument = "12345678",
            SignatureName = "Administración Dexter Laundry",
            SignaturePosition = "Jefe de Operaciones",
            SignaturePhone = "999999999",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
