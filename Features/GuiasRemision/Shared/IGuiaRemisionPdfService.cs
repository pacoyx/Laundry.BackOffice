public interface IGuiaRemisionPdfService
{
    Task<byte[]> GenerateAsync(long guiaRemisionId, CancellationToken cancellationToken = default);
}
