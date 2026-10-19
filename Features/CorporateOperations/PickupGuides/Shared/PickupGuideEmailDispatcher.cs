using Microsoft.EntityFrameworkCore;

internal static class PickupGuideEmailDispatcher
{
    public static async Task SendReceiptEmailAsync(
        PickupGuide guide,
        byte[] pdfBytes,
        string attachmentFileName,
        string actionVerb,
        RecepcionDbContext db,
        IEmailService emailService,
        DateTime now)
    {
        try
        {
            var recipients = await db.CustomerContacts
                .Where(c => c.CustomerId == guide.CustomerId && c.ReceivePickupGuideEmails && c.Email != null && c.Email != "")
                .Select(c => c.Email!)
                .ToListAsync();

            if (recipients.Count == 0)
            {
                guide.EmailDeliveryStatus = PickupGuideEmailStatus.Failed;
                guide.LastEmailError = "Sin destinatarios configurados (ningún contacto del cliente tiene ReceivePickupGuideEmails activo).";
                return;
            }

            var message = new EmailMessage(
                recipients,
                $"Constancia de Recojo - {guide.GuideNumber}",
                $"<p>{actionVerb} la constancia de recojo <strong>{guide.GuideNumber}</strong>.</p>",
                new[] { new EmailAttachment(attachmentFileName, pdfBytes, "application/pdf") }
            );

            await emailService.SendAsync(message, EmailOperationCodes.CorporateOperations);

            guide.EmailDeliveryStatus = PickupGuideEmailStatus.Sent;
            guide.EmailSentAt = now;
            guide.LastEmailError = null;
        }
        catch (Exception ex)
        {
            guide.EmailDeliveryStatus = PickupGuideEmailStatus.Failed;
            guide.LastEmailError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
        }
    }
}
