public interface IInvoicePdfService
{
    Task<byte[]> GenerateAsync(long invoiceId, CancellationToken cancellationToken = default);
}
