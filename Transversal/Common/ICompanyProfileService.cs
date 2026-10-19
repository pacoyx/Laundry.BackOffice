public interface ICompanyProfileService
{
    Task<Company?> GetCompanyAsync(CancellationToken cancellationToken = default);
    Task<byte[]?> GetCompanyLogoBytesAsync(CancellationToken cancellationToken = default);
}
