using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// Envia el comprobante a NubeFact (www.nubefact.com/integracion) por su API REST.
// Se activa con ElectronicInvoicing:Provider = "NubeFact" (ver ServiceExtensions.cs).
// A proposito NUNCA relanza la excepcion: un fallo aqui no debe tumbar el job de Hangfire
// ni disparar un reintento automatico, porque reintentar podria reenviar el mismo
// comprobante dos veces a SUNAT. Cada intento (exitoso o no) queda registrado en
// ElectronicInvoiceSubmissions para poder revisarlo/reenviar manualmente despues.
public class NubeFactElectronicInvoicingService : IElectronicInvoicingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;
    private readonly RecepcionDbContext _db;
    private readonly NubeFactSettings _settings;
    private readonly InvoiceSettings _invoiceSettings;
    private readonly IAppLogger<NubeFactElectronicInvoicingService> _logger;

    public NubeFactElectronicInvoicingService(
        HttpClient httpClient,
        RecepcionDbContext db,
        IOptions<ElectronicInvoicingSettings> electronicInvoicingSettings,
        IOptions<InvoiceSettings> invoiceSettings,
        IAppLogger<NubeFactElectronicInvoicingService> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _settings = electronicInvoicingSettings.Value.NubeFact;
        _invoiceSettings = invoiceSettings.Value;
        _logger = logger;
    }

    public async Task SubmitInvoiceAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        var submission = new ElectronicInvoiceSubmission
        {
            InvoiceId = invoiceId,
            Provider = "NubeFact",
            SentAt = DateTime.UtcNow
        };

        try
        {
            var invoice = await _db.Invoices
                .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

            if (invoice is null)
            {
                throw new InvalidOperationException($"Factura {invoiceId} no encontrada.");
            }

            var documentType = await _db.TypeDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == invoice.DocumentTypeId, cancellationToken);

            if (documentType is null || !NubeFactRequestMapper.TryGetTipoDeComprobante(documentType.DocId, out var tipoDeComprobante))
            {
                throw new InvalidOperationException(
                    $"El tipo de documento de la factura {invoiceId} no es Factura ni Boleta; no se puede enviar a NubeFact.");
            }

            var productIds = invoice.Details.Select(d => d.ProductId).Distinct().ToList();
            var products = await _db.Products
                .AsNoTracking()
                .Include(p => p.UnitMeasurement)
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var payload = NubeFactRequestMapper.Build(invoice, tipoDeComprobante, _invoiceSettings.IgvRate, products);

            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", $"token={_settings.Token}");

            var httpResponse = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            submission.RawResponseJson = responseBody;

            var nubeFactResponse = JsonSerializer.Deserialize<NubeFactResponse>(responseBody, JsonOptions);

            if (!httpResponse.IsSuccessStatusCode || nubeFactResponse is null || !string.IsNullOrEmpty(nubeFactResponse.Errors))
            {
                submission.Success = false;
                submission.ErrorMessage = nubeFactResponse?.Errors ?? $"NubeFact respondió {httpResponse.StatusCode}.";
                _logger.LogError("NubeFact rechazó la factura {0}: {1}", invoiceId, submission.ErrorMessage);
            }
            else
            {
                submission.Success = true;
                submission.AcceptedBySunat = nubeFactResponse.AceptadaPorSunat;
                submission.SunatDescription = nubeFactResponse.SunatDescription;
                submission.SunatResponseCode = nubeFactResponse.SunatResponseCode;
                submission.DocumentUrl = nubeFactResponse.Url;
                _logger.LogInformacion("Factura {0} enviada a NubeFact correctamente. Aceptada por SUNAT: {1}", invoiceId, nubeFactResponse.AceptadaPorSunat);
            }
        }
        catch (Exception ex)
        {
            submission.Success = false;
            submission.ErrorMessage = ex.Message;
            _logger.LogError("Error enviando la factura {0} a NubeFact: {1}", invoiceId, ex.Message);
        }

        await _db.ElectronicInvoiceSubmissions.AddAsync(submission, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
