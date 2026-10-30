using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

// Envia SMS llamando directo a la REST API de Twilio (sin el SDK oficial, para no sumar una
// dependencia nueva solo por esto). Activar con Sms:Provider = "Twilio" en configuracion,
// una vez se tenga una cuenta Twilio real (ver Twilio:AccountSid/AuthToken/FromPhoneNumber).
public class TwilioSmsService : ISmsService
{
    private const string ApiBaseUrl = "https://api.twilio.com/2010-04-01";

    private readonly HttpClient _httpClient;
    private readonly TwilioSettings _settings;
    private readonly IAppLogger<TwilioSmsService> _logger;

    public TwilioSmsService(HttpClient httpClient, IOptions<TwilioSettings> settings, IAppLogger<TwilioSmsService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string phone, string message)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}"));

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiBaseUrl}/Accounts/{_settings.AccountSid}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = phone,
                ["From"] = _settings.FromPhoneNumber,
                ["Body"] = message
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error al enviar SMS por Twilio a {0}: {1} - {2}", phone, response.StatusCode, body);
            throw new InvalidOperationException($"Twilio respondió {response.StatusCode} al enviar el SMS.");
        }
    }
}
