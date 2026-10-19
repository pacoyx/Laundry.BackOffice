using Microsoft.EntityFrameworkCore;

// El modulo Features/Company permite registrar varias empresas (UsuarioId por fila), pero
// en este sistema solo se usa para los datos fiscales propios de Lavanderia Kyo: se asume
// una sola empresa activa, la primera creada.
public class CompanyProfileService : ICompanyProfileService
{
    private readonly RecepcionDbContext _db;

    public CompanyProfileService(RecepcionDbContext db)
    {
        _db = db;
    }

    public Task<Company?> GetCompanyAsync(CancellationToken cancellationToken = default) =>
        _db.Companies
            .AsNoTracking()
            .Where(c => c.Status == "A")
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);
}
