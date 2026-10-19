public interface ICompanyProfileService
{
    Task<Company?> GetCompanyAsync(CancellationToken cancellationToken = default);
}
