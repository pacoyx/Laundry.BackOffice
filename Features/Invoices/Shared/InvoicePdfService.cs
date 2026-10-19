using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;

public class InvoicePdfService : IInvoicePdfService
{
    private readonly RecepcionDbContext _db;
    private readonly ICompanyProfileService _companyProfileService;
    private readonly InvoicePrintSettings _invoicePrintSettings;

    public InvoicePdfService(
        RecepcionDbContext db,
        ICompanyProfileService companyProfileService,
        IOptions<InvoicePrintSettings> invoicePrintSettings)
    {
        _db = db;
        _companyProfileService = companyProfileService;
        _invoicePrintSettings = invoicePrintSettings.Value;
    }

    public async Task<byte[]> GenerateAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

        if (invoice is null)
        {
            throw new InvalidOperationException($"Factura {invoiceId} no encontrada.");
        }

        var branchAddress = await _db.BranchSales
            .Where(x => x.Id == invoice.BranchId)
            .Select(x => x.Address)
            .FirstOrDefaultAsync(cancellationToken);

        var company = await _companyProfileService.GetCompanyAsync(cancellationToken);
        var logoImage = await _companyProfileService.GetCompanyLogoBytesAsync(cancellationToken);

        var document = new InvoicePdfDocument(invoice, company, _invoicePrintSettings, branchAddress, logoImage);
        return document.GeneratePdf();
    }
}
