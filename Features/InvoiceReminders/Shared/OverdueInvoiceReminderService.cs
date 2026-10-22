using Microsoft.EntityFrameworkCore;

public class OverdueInvoiceReminderService : IOverdueInvoiceReminderService
{
    private readonly RecepcionDbContext _db;
    private readonly IEmailService _emailService;

    public OverdueInvoiceReminderService(RecepcionDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task ExecuteAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default)
    {
        var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
        var today = now.Date;

        var config = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == scheduledEmailNotificationId, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException($"Configuración de correo programado {scheduledEmailNotificationId} no encontrada.");
        }

        // Mismo criterio de "factura vencida" que usa el dashboard de cuentas por cobrar
        // (GetOverdueInvoicesEndpoint), mas la exclusion de facturas puntualmente suprimidas.
        var overdueInvoices = await _db.Invoices
            .AsNoTracking()
            .Include(x => x.Customer).ThenInclude(c => c!.BillingProfile)
            .Include(x => x.Customer).ThenInclude(c => c!.Contacts)
            .Where(x => x.DueDate < today && x.Balance > 0 && x.Status != InvoiceStatus.Cancelled && !x.ReminderSuppressed)
            .ToListAsync(cancellationToken);

        foreach (var group in overdueInvoices.GroupBy(x => x.CustomerId))
        {
            var customer = group.First().Customer;
            var invoices = group.ToList();

            var status = InvoiceReminderLogStatus.Sent;
            string? skipReason = null;
            string? errorMessage = null;
            var recipients = new List<string>();

            if (customer.BillingProfile is not null && !customer.BillingProfile.RemindersEnabled)
            {
                status = InvoiceReminderLogStatus.Skipped;
                skipReason = string.IsNullOrWhiteSpace(customer.BillingProfile.RemindersSuppressedReason)
                    ? "Cliente suprimido"
                    : $"Cliente suprimido: {customer.BillingProfile.RemindersSuppressedReason}";
            }
            else
            {
                recipients = customer.Contacts
                    .Where(c => c.ReceiveInvoices && !string.IsNullOrWhiteSpace(c.Email))
                    .Select(c => c.Email!)
                    .Distinct()
                    .ToList();

                if (recipients.Count == 0)
                {
                    status = InvoiceReminderLogStatus.Skipped;
                    skipReason = "Sin destinatarios configurados (ningún contacto con ReceiveInvoices activo).";
                }
                else
                {
                    try
                    {
                        var (subject, body) = BuildEmail(config, customer, invoices, today);
                        var message = new EmailMessage(recipients, subject, body, Array.Empty<EmailAttachment>());
                        await _emailService.SendAsync(message, EmailOperationCodes.Collections, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        status = InvoiceReminderLogStatus.Failed;
                        errorMessage = ex.Message;
                    }
                }
            }

            _db.InvoiceReminderLogs.Add(new InvoiceReminderLog
            {
                ScheduledEmailNotificationId = scheduledEmailNotificationId,
                CustomerId = customer.Id,
                RunAt = now,
                Status = status,
                SkipReason = skipReason,
                ErrorMessage = errorMessage,
                Recipients = string.Join(", ", recipients),
                InvoiceCount = invoices.Count,
                TotalAmount = invoices.Sum(x => x.Balance),
                CreatedAt = now,
                Details = invoices.Select(inv => new InvoiceReminderLogDetail
                {
                    InvoiceId = inv.Id,
                    Balance = inv.Balance,
                    DaysOverdue = (int)(today - inv.DueDate.Date).TotalDays
                }).ToList()
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(string Subject, string Body)> PreviewAsync(int scheduledEmailNotificationId, int customerId, CancellationToken cancellationToken = default)
    {
        var config = await _db.ScheduledEmailNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == scheduledEmailNotificationId, cancellationToken);

        if (config is null)
        {
            throw new InvalidOperationException($"Configuración de correo programado {scheduledEmailNotificationId} no encontrada.");
        }

        var (customer, invoices, today) = await GetCustomerOverdueInvoicesAsync(customerId, cancellationToken);

        if (invoices.Count == 0)
        {
            throw new InvalidOperationException("El cliente no tiene facturas vencidas actualmente.");
        }

        return BuildEmail(config, customer, invoices, today);
    }

    public async Task SendTestAsync(int scheduledEmailNotificationId, int customerId, string testEmail, CancellationToken cancellationToken = default)
    {
        var (subject, body) = await PreviewAsync(scheduledEmailNotificationId, customerId, cancellationToken);
        var message = new EmailMessage(new[] { testEmail }, $"[PRUEBA] {subject}", body, Array.Empty<EmailAttachment>());
        await _emailService.SendAsync(message, EmailOperationCodes.Collections, cancellationToken);
    }

    private async Task<(Customer Customer, List<Invoice> Invoices, DateTime Today)> GetCustomerOverdueInvoicesAsync(int customerId, CancellationToken cancellationToken)
    {
        var today = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time")).Date;

        var customer = await _db.Customers
            .AsNoTracking()
            .Include(x => x.BillingProfile)
            .FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);

        if (customer is null)
        {
            throw new InvalidOperationException($"Cliente {customerId} no encontrado.");
        }

        var invoices = await _db.Invoices
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId && x.DueDate < today && x.Balance > 0 && x.Status != InvoiceStatus.Cancelled && !x.ReminderSuppressed)
            .ToListAsync(cancellationToken);

        return (customer, invoices, today);
    }

    private static (string Subject, string Body) BuildEmail(ScheduledEmailNotification config, Customer customer, List<Invoice> invoices, DateTime today)
    {
        var customerName = PickupGuidesEndpoints.GetCustomerDisplayName(customer);
        var totalAmount = invoices.Sum(x => x.Balance);
        var invoiceListHtml = BuildInvoiceListHtml(invoices, today);

        string Resolve(string template) => template
            .Replace("{CustomerName}", customerName)
            .Replace("{InvoiceCount}", invoices.Count.ToString())
            .Replace("{TotalOverdueAmount}", totalAmount.ToString("N2"))
            .Replace("{InvoiceList}", invoiceListHtml)
            .Replace("{SignatureName}", config.SignatureName)
            .Replace("{SignaturePosition}", config.SignaturePosition)
            .Replace("{SignaturePhone}", config.SignaturePhone);

        return (Resolve(config.SubjectTemplate), Resolve(config.BodyTemplate));
    }

    private static string BuildInvoiceListHtml(List<Invoice> invoices, DateTime today)
    {
        var rows = invoices
            .OrderByDescending(x => x.DueDate)
            .Select(x => $"<tr><td>{x.Serie}-{x.Numero}</td><td>{x.DueDate:dd/MM/yyyy}</td><td>{(today - x.DueDate.Date).Days}</td><td>{x.Balance:N2}</td></tr>");

        return "<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">"
            + "<tr><th>Factura</th><th>Vencimiento</th><th>Días de atraso</th><th>Saldo</th></tr>"
            + string.Join("", rows)
            + "</table>";
    }
}
