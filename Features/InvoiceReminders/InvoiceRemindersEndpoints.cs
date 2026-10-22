public static class InvoiceRemindersEndpoints
{
    public static void MapInvoiceReminders(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoice-reminders")
            .WithTags("InvoiceReminders")
            .RequirePermission(PermissionCodes.Invoices);

        group.MapExecuteInvoiceReminders();
        group.MapPreviewInvoiceReminder();
        group.MapSendTestInvoiceReminder();
        group.MapGetInvoiceReminderLogs();
    }

    internal static InvoiceReminderLogResponse ToResponse(InvoiceReminderLog log) => new(
        log.Id, log.CustomerId, PickupGuidesEndpoints.GetCustomerDisplayName(log.Customer!), log.RunAt,
        log.Status, log.SkipReason, log.ErrorMessage, log.Recipients, log.InvoiceCount, log.TotalAmount, log.CreatedAt,
        log.Details
            .Select(d => new InvoiceReminderLogDetailResponse(d.InvoiceId, $"{d.Invoice!.Serie}-{d.Invoice.Numero}", d.Balance, d.DaysOverdue))
            .ToList()
    );
}
