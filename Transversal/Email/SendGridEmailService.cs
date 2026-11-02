using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

// Envia correo llamando directo a la API HTTP v3 de SendGrid (mismo criterio que TwilioSmsService:
// sin SDK oficial, para no sumar una dependencia nueva solo por esto). Necesario porque
// DigitalOcean bloquea los puertos SMTP (25/465/587) en todos sus droplets -SmtpEmailService ya no
// puede funcionar ahi bajo ninguna configuracion. Activar con Email:Provider = "SendGrid" y
// completar la seccion SendGrid (ApiKey/FromEmail/FromName). A diferencia de SmtpEmailService, usa
// un remitente unico global en vez de una cuenta distinta por Operacion.
public class SendGridEmailService : IEmailService
{
    private const string ApiUrl = "https://api.sendgrid.com/v3/mail/send";

    private readonly HttpClient _httpClient;
    private readonly SendGridSettings _settings;
    private readonly IAppLogger<SendGridEmailService> _logger;

    public SendGridEmailService(HttpClient httpClient, IOptions<SendGridSettings> settings, IAppLogger<SendGridEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, string operacion, CancellationToken cancellationToken = default)
    {
        var personalization = new Dictionary<string, object>
        {
            ["to"] = message.To.Select(email => new { email }).ToArray()
        };

        if (message.Bcc is not null && message.Bcc.Any())
        {
            personalization["bcc"] = message.Bcc.Select(email => new { email }).ToArray();
        }

        var payload = new Dictionary<string, object>
        {
            ["personalizations"] = new object[] { personalization },
            ["from"] = new { email = _settings.FromEmail, name = _settings.FromName },
            ["subject"] = message.Subject,
            ["content"] = new[] { new { type = "text/html", value = message.HtmlBody } },
            ["categories"] = new[] { operacion }
        };

        var attachments = message.Attachments
            .Select(a => new
            {
                content = Convert.ToBase64String(a.Content),
                filename = a.FileName,
                type = a.ContentType,
                disposition = "attachment"
            })
            .ToArray();

        if (attachments.Length > 0)
        {
            payload["attachments"] = attachments;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Error al enviar correo por SendGrid (operacion={0}): {1} - {2}", operacion, response.StatusCode, body);
            throw new InvalidOperationException($"SendGrid respondió {response.StatusCode} al enviar el correo.");
        }
    }
}
