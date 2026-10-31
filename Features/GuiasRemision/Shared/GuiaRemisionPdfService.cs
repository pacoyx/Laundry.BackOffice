using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public class GuiaRemisionPdfService : IGuiaRemisionPdfService
{
    private readonly RecepcionDbContext _db;
    private readonly ICompanyProfileService _companyProfileService;

    public GuiaRemisionPdfService(
        RecepcionDbContext db,
        ICompanyProfileService companyProfileService)
    {
        _db = db;
        _companyProfileService = companyProfileService;
    }

    public async Task<byte[]> GenerateAsync(long guiaRemisionId, CancellationToken cancellationToken = default)
    {
        var guia = await _db.GuiasRemision
            .AsNoTracking()
            .Include(x => x.Invoice).ThenInclude(i => i.Customer).ThenInclude(c => c.BillingProfile)
            .Include(x => x.Invoice).ThenInclude(i => i.Details)
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == guiaRemisionId, cancellationToken);

        if (guia is null)
        {
            throw new InvalidOperationException($"Guía de remisión {guiaRemisionId} no encontrada.");
        }

        var company = await _companyProfileService.GetCompanyAsync(cancellationToken);
        var logoImage = await _companyProfileService.GetCompanyLogoBytesAsync(cancellationToken);

        var document = new GuiaRemisionPdfDocument(guia, company, logoImage);
        return document.GeneratePdf();
    }
}
