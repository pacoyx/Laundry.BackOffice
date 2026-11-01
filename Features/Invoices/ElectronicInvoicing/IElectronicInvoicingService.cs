public interface IElectronicInvoicingService
{
    Task SubmitInvoiceAsync(long invoiceId, CancellationToken cancellationToken = default);
}
