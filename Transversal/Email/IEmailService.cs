public record EmailAttachment(string FileName, byte[] Content, string ContentType);

public record EmailMessage(IEnumerable<string> To, string Subject, string HtmlBody, IEnumerable<EmailAttachment> Attachments);

public interface IEmailService
{
    Task SendAsync(EmailMessage message, string operacion, CancellationToken cancellationToken = default);
}
