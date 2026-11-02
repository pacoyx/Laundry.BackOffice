using System.Net.Http.Json;
using Microsoft.Extensions.Options;

// Envia correo llamando directo a la API transaccional v3 de Brevo (mismo criterio que
// SendGridEmailService/TwilioSmsService: sin SDK oficial). Alternativa a SendGrid con plan
// gratuito permanente (300 correos/dia, sin limite de tiempo) en vez de un trial de 60 dias.
// Activar con Email:Provider = "Brevo" y completar la seccion Brevo (ApiKey/FromEmail/FromName).
public class BrevoEmailService : IEmailService
{
    private const string ApiUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly HttpClient _httpClient;
    private readonly BrevoSettings _settings;
    private readonly IAppLogger<BrevoEmailService> _logger;

    public BrevoEmailService(HttpClient httpClient, IOptions<BrevoSettings> settings, IAppLogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, string operacion, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object>
        {
            ["sender"] = new { email = _settings.FromEmail, name = _settings.FromName },
            ["to"] = message.To.Select(email => new { email }).ToArray(),
            ["subject"] = message.Subject,
            ["htmlContent"] = message.HtmlBody,
            ["tags"] = new[] { operacion }
        };

        if (message.Bcc is not null && message.Bcc.Any())
        {
            payload["bcc"] = message.Bcc.Select(email => new { email }).ToArray();
        }

        var attachments = message.Attachments
            .Select(a => new
            {
                content = Convert.ToBase64String(a.Content),
                name = a.FileName
            })
            .ToArray();

        if (attachments.Length > 0)
        {
            payload["attachment"] = attachments;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Error al enviar correo por Brevo (operacion={0}): {1} - {2}", operacion, response.StatusCode, body);
            throw new InvalidOperationException($"Brevo respondió {response.StatusCode} al enviar el correo.");
        }
    }
}
