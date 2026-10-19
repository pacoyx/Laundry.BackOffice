using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public class PickupGuideReceiptPdfService : IPickupGuideReceiptPdfService
{
    private readonly RecepcionDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly IImageCompressor _imageCompressor;
    private readonly ICompanyProfileService _companyProfileService;

    public PickupGuideReceiptPdfService(
        RecepcionDbContext db,
        IFileStorageService storage,
        IImageCompressor imageCompressor,
        ICompanyProfileService companyProfileService)
    {
        _db = db;
        _storage = storage;
        _imageCompressor = imageCompressor;
        _companyProfileService = companyProfileService;
    }

    public async Task<byte[]> GenerateAsync(long pickupGuideId, CancellationToken cancellationToken = default)
    {
        var guide = await _db.PickupGuides
            .AsNoTracking()
            .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
            .Include(x => x.Employee)
            .Include(x => x.Vehicle)
            .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
            .FirstOrDefaultAsync(x => x.Id == pickupGuideId, cancellationToken);

        if (guide is null)
        {
            throw new InvalidOperationException($"Guía de recojo {pickupGuideId} no encontrada.");
        }

        var attachments = await _db.PickupGuideAttachments
            .AsNoTracking()
            .Where(x => x.PickupGuideId == pickupGuideId && x.IsCurrent)
            .ToListAsync(cancellationToken);

        byte[]? signatureImage = null;
        var signatureAttachment = attachments.FirstOrDefault(x => x.AttachmentType == PickupGuideAttachmentType.Signature);
        if (signatureAttachment is not null)
        {
            signatureImage = await ReadAllBytesAsync(signatureAttachment.FilePath, cancellationToken);
        }

        var photoImages = new List<byte[]>();
        foreach (var photoAttachment in attachments.Where(x => x.AttachmentType == PickupGuideAttachmentType.Photo))
        {
            var bytes = await ReadAllBytesAsync(photoAttachment.FilePath, cancellationToken);
            photoImages.Add(_imageCompressor.CompressToJpeg(bytes));
        }

        var revisions = await _db.PickupGuideRevisions
            .AsNoTracking()
            .Where(x => x.PickupGuideId == pickupGuideId)
            .Include(x => x.Employee)
            .Include(x => x.DetailRevisions).ThenInclude(d => d.PickupGuideDetail).ThenInclude(d => d.ServiceItem)
            .Include(x => x.Photos)
            .OrderBy(x => x.RevisionDate)
            .ToListAsync(cancellationToken);

        var originalQuantityByDetailId = BuildOriginalQuantitiesByDetailId(revisions);
        var addedItemDetailIds = revisions
            .SelectMany(r => r.DetailRevisions)
            .Where(d => d.ActionType == PickupGuideRevisionActionType.AddedItem)
            .Select(d => d.PickupGuideDetailId)
            .ToHashSet();

        var revisionPdfData = new List<PickupGuideRevisionPdfData>();
        foreach (var revision in revisions)
        {
            var revisionPhotoImages = new List<byte[]>();
            foreach (var photo in revision.Photos)
            {
                var bytes = await ReadAllBytesAsync(photo.FilePath, cancellationToken);
                revisionPhotoImages.Add(_imageCompressor.CompressToJpeg(bytes));
            }

            revisionPdfData.Add(new PickupGuideRevisionPdfData(revision, revisionPhotoImages));
        }

        var company = await _companyProfileService.GetCompanyAsync(cancellationToken);

        var document = new PickupGuideReceiptPdfDocument(
            guide, company?.NameComercial ?? string.Empty, signatureImage, photoImages, revisionPdfData, originalQuantityByDetailId, addedItemDetailIds);
        return document.GeneratePdf();
    }

    // El valor "declarado" de un detalle es la cantidad/peso antes de cualquier revisión:
    // la primera revisión en el tiempo que lo tocó ya trae ese valor en OriginalQuantity.
    // Si nunca fue revisado, el valor actual del detalle ya es el declarado.
    private static Dictionary<long, decimal> BuildOriginalQuantitiesByDetailId(List<PickupGuideRevision> revisions)
    {
        var result = new Dictionary<long, decimal>();

        foreach (var detailRevision in revisions.SelectMany(r => r.DetailRevisions))
        {
            if (!result.ContainsKey(detailRevision.PickupGuideDetailId))
            {
                result[detailRevision.PickupGuideDetailId] = detailRevision.OriginalQuantity;
            }
        }

        return result;
    }

    private async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = await _storage.OpenReadAsync(filePath, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }
}
