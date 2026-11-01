// Implementacion no-op mientras no se active un proveedor real (ElectronicInvoicing:Provider
// vacio). No deberia invocarse nunca en la practica porque el hook en CreateInvoiceEndpoint
// solo encola el job cuando Provider == "NubeFact", pero queda registrada por si en el futuro
// algo mas inyecta IElectronicInvoicingService directamente.
public class LoggingElectronicInvoicingService : IElectronicInvoicingService
{
    private readonly IAppLogger<LoggingElectronicInvoicingService> _logger;

    public LoggingElectronicInvoicingService(IAppLogger<LoggingElectronicInvoicingService> logger)
    {
        _logger = logger;
    }

    public Task SubmitInvoiceAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformacion("Facturacion electronica deshabilitada (stub). Factura {0} no fue enviada a ningun proveedor.", invoiceId);
        return Task.CompletedTask;
    }
}
