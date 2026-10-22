using Microsoft.EntityFrameworkCore;

// Configuracion inicial del recordatorio de facturas vencidas. A diferencia del aviso de
// chofer, esta se siembra DESHABILITADA (IsEnabled = false): le llega a clientes reales,
// asi que el admin debe revisar la plantilla y habilitarla conscientemente.
public class InvoiceReminderNotificationSeeder : IApplicationSeeder
{
    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var exists = await context.ScheduledEmailNotifications
            .AnyAsync(x => x.Code == InvoiceReminderConstants.ConfigCode, cancellationToken);

        if (exists)
        {
            return;
        }

        var bodyTemplate = string.Join("<br/>", new[]
        {
            "Estimados {CustomerName},",
            "",
            "Esperamos que se encuentren bien. Les escribimos para recordarles que registran",
            "{InvoiceCount} factura(s) pendiente(s) de pago, por un monto total de S/ {TotalOverdueAmount}.",
            "",
            "{InvoiceList}",
            "",
            "Agradeceríamos regularizar el pago a la brevedad posible. Si ya realizaron el",
            "pago, por favor hacer caso omiso a este mensaje.",
            "",
            "Quedamos atentos a cualquier consulta.",
            "",
            "Atentamente,",
            "",
            "{SignatureName}",
            "{SignaturePosition}",
            "{SignaturePhone}"
        });

        context.ScheduledEmailNotifications.Add(new ScheduledEmailNotification
        {
            Code = InvoiceReminderConstants.ConfigCode,
            Name = "Recordatorio de Facturas Vencidas",
            RecipientMode = ScheduledEmailRecipientMode.Dynamic,
            CronExpression = CronScheduleHelper.BuildCronExpression(8, 0, new[]
            {
                ScheduledWeekday.Monday, ScheduledWeekday.Tuesday, ScheduledWeekday.Wednesday,
                ScheduledWeekday.Thursday, ScheduledWeekday.Friday
            }),
            SubjectTemplate = "Recordatorio de pago - Factura(s) vencida(s)",
            BodyTemplate = bodyTemplate,
            SignatureName = "Administración Dexter Laundry",
            SignaturePosition = "Área de Cobranzas",
            SignaturePhone = "999999999",
            IsEnabled = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
