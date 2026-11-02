using System.Net.Http.Json;
using Microsoft.Extensions.Options;

// Envia SMS llamando directo a la API transaccional de SMS de Brevo (mismo criterio que
// TwilioSmsService: sin SDK oficial). A diferencia del correo, el SMS en Brevo tiene costo por
// mensaje (no hay plan gratuito permanente) -activar con Sms:Provider = "Brevo" solo cuando se
// decida asumir ese costo.
public class BrevoSmsService : ISmsService
{
    private const string ApiUrl = "https://api.brevo.com/v3/transactionalSMS/sms";

    private readonly HttpClient _httpClient;
    private readonly BrevoSmsSettings _settings;
    private readonly IAppLogger<BrevoSmsService> _logger;

    public BrevoSmsService(HttpClient httpClient, IOptions<BrevoSmsSettings> settings, IAppLogger<BrevoSmsService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string phone, string message)
    {
        var payload = new
        {
            sender = _settings.Sender,
            recipient = phone,
            content = message,
            type = "transactional"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", _settings.ApiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error al enviar SMS por Brevo a {0}: {1} - {2}", phone, response.StatusCode, body);
            throw new InvalidOperationException($"Brevo respondió {response.StatusCode} al enviar el SMS.");
        }
    }
}
