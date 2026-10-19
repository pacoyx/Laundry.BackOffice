using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

public class SmtpEmailService : IEmailService
{
    private readonly RecepcionDbContext _db;
    private readonly ISymmetricEncryptionService _encryptionService;

    public SmtpEmailService(RecepcionDbContext db, ISymmetricEncryptionService encryptionService)
    {
        _db = db;
        _encryptionService = encryptionService;
    }

    public async Task SendAsync(EmailMessage message, string operacion, CancellationToken cancellationToken = default)
    {
        var account = await _db.EmailAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Operacion == operacion && x.IsActive, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException($"No hay una cuenta de correo activa configurada para la operación '{operacion}'.");
        }

        if (string.IsNullOrEmpty(account.Password))
        {
            throw new InvalidOperationException($"La cuenta de correo '{operacion}' no tiene una contraseña configurada.");
        }

        var password = _encryptionService.Decrypt(account.Password);

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(MailboxAddress.Parse(account.UserName));

        foreach (var recipient in message.To)
        {
            mimeMessage.To.Add(MailboxAddress.Parse(recipient));
        }

        mimeMessage.Subject = message.Subject;

        var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
        foreach (var attachment in message.Attachments)
        {
            builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        mimeMessage.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        var secureSocketOptions = account.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

        await client.ConnectAsync(account.Host, account.Port, secureSocketOptions, cancellationToken);
        await client.AuthenticateAsync(account.UserName, password, cancellationToken);
        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
