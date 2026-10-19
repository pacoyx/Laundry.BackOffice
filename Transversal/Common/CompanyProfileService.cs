using Microsoft.EntityFrameworkCore;

// El modulo Features/Company permite registrar varias empresas (UsuarioId por fila), pero
// en este sistema solo se usa para los datos fiscales propios de Lavanderia Kyo: se asume
// una sola empresa activa, la primera creada.
public class CompanyProfileService : ICompanyProfileService
{
    private readonly RecepcionDbContext _db;
    private readonly IFileStorageService _storage;

    public CompanyProfileService(RecepcionDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public Task<Company?> GetCompanyAsync(CancellationToken cancellationToken = default) =>
        _db.Companies
            .AsNoTracking()
            .Where(c => c.Status == "A")
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<byte[]?> GetCompanyLogoBytesAsync(CancellationToken cancellationToken = default)
    {
        var company = await GetCompanyAsync(cancellationToken);
        if (company is null || string.IsNullOrEmpty(company.Logo))
        {
            return null;
        }

        await using var stream = await _storage.OpenReadAsync(company.Logo, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }
}
