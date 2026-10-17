using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

public class ValuationReportService : IValuationReportService
{
    private readonly RecepcionDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly IImageCompressor _imageCompressor;

    public ValuationReportService(RecepcionDbContext db, IFileStorageService storage, IImageCompressor imageCompressor)
    {
        _db = db;
        _storage = storage;
        _imageCompressor = imageCompressor;
    }

    public async Task<ValuationReportResponse> GetDataAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default)
    {
        var guides = await LoadGuidesAsync(filter, cancellationToken);
        var customerName = await ResolveCustomerNameAsync(filter.CustomerId, cancellationToken);

        return new ValuationReportResponse
        {
            CustomerName = customerName,
            Guides = guides.Select(ToGuideRow).OrderBy(x => x.PickupDate).ToList(),
            Pivot = BuildPivot(guides),
            PhotoCount = guides.Sum(g => g.Attachments.Count(a => a.AttachmentType == PickupGuideAttachmentType.PhysicalGuidePhoto))
        };
    }

    public async Task<byte[]> GeneratePhotosPdfAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default)
    {
        var guides = await LoadGuidesAsync(filter, cancellationToken);
        var customerName = await ResolveCustomerNameAsync(filter.CustomerId, cancellationToken);

        var photoItems = new List<ValuationReportPhotoItem>();
        foreach (var guide in guides.OrderBy(g => g.PickupDate).ThenBy(g => g.GuideNumber))
        {
            var physicalGuidePhotos = guide.Attachments
                .Where(a => a.AttachmentType == PickupGuideAttachmentType.PhysicalGuidePhoto)
                .OrderBy(a => a.CreatedAt);

            foreach (var attachment in physicalGuidePhotos)
            {
                var bytes = await ReadAllBytesAsync(attachment.FilePath, cancellationToken);
                photoItems.Add(new ValuationReportPhotoItem
                {
                    GuideNumber = guide.GuideNumber,
                    PickupDate = guide.PickupDate,
                    ImageBytes = _imageCompressor.CompressToJpeg(bytes)
                });
            }
        }

        var document = new ValuationReportPhotosPdfDocument(photoItems, customerName, filter);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateSummaryPdfAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default)
    {
        var guides = await LoadGuidesAsync(filter, cancellationToken);
        var customerName = await ResolveCustomerNameAsync(filter.CustomerId, cancellationToken);
        var pivot = BuildPivot(guides);

        var document = new ValuationReportSummaryPdfDocument(pivot, customerName, filter);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateSummaryExcelAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default)
    {
        var guides = await LoadGuidesAsync(filter, cancellationToken);
        var customerName = await ResolveCustomerNameAsync(filter.CustomerId, cancellationToken);
        var pivot = BuildPivot(guides);

        return ValuationReportSummaryExcelBuilder.Build(pivot, customerName, filter);
    }

    private async Task<List<PickupGuide>> LoadGuidesAsync(ValuationReportFilter filter, CancellationToken cancellationToken)
    {
        var fromDate = filter.FromDate.Date;
        var toDate = filter.ToDate.Date;

        return await _db.PickupGuides
            .AsNoTracking()
            .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
            .Include(x => x.Attachments)
            .Where(x => x.CustomerId == filter.CustomerId
                && x.PickupDate.Date >= fromDate
                && x.PickupDate.Date <= toDate
                && x.Status != PickupGuideStatus.Cancelled)
            .ToListAsync(cancellationToken);
    }

    private async Task<string> ResolveCustomerNameAsync(int customerId, CancellationToken cancellationToken)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .Include(x => x.BillingProfile)
            .FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);

        return customer is null ? "-" : PickupGuidesEndpoints.GetCustomerDisplayName(customer);
    }

    private static ValuationReportGuideRowDto ToGuideRow(PickupGuide guide) => new()
    {
        Id = guide.Id,
        GuideNumber = guide.GuideNumber,
        PickupDate = guide.PickupDate,
        Status = guide.Status.ToString(),
        TotalAmount = guide.TotalAmount,
        HasPhysicalGuidePhoto = guide.Attachments.Any(a => a.AttachmentType == PickupGuideAttachmentType.PhysicalGuidePhoto)
    };

    // Pivote: filas = fecha de la guía, columnas = servicio, valor = cantidad (o peso, según
    // el servicio). Varias guías del mismo día y servicio se suman en la misma celda.
    private static ValuationReportPivotDto BuildPivot(List<PickupGuide> guides)
    {
        var services = guides
            .SelectMany(g => g.Details)
            .Select(d => new ValuationReportServiceColumnDto
            {
                ServiceItemId = d.ServiceItemId,
                ServiceName = d.ServiceItem.Name,
                IsPricedByWeight = d.ServiceItem.IsPeso == "S"
            })
            .DistinctBy(s => s.ServiceItemId)
            .OrderBy(s => s.ServiceName)
            .ToList();

        var rowsByDate = new Dictionary<DateTime, ValuationReportPivotRowDto>();

        foreach (var guide in guides)
        {
            var date = guide.PickupDate.Date;
            if (!rowsByDate.TryGetValue(date, out var row))
            {
                row = new ValuationReportPivotRowDto { Date = date };
                rowsByDate[date] = row;
            }

            foreach (var detail in guide.Details)
            {
                var isPricedByWeight = detail.ServiceItem.IsPeso == "S";
                var basis = isPricedByWeight ? detail.Weight ?? 0 : detail.Quantity;

                row.QuantitiesByServiceItemId[detail.ServiceItemId] =
                    row.QuantitiesByServiceItemId.GetValueOrDefault(detail.ServiceItemId) + basis;
            }
        }

        foreach (var row in rowsByDate.Values)
        {
            row.RowTotal = row.QuantitiesByServiceItemId.Values.Sum();
        }

        var columnTotals = services.ToDictionary(
            s => s.ServiceItemId,
            s => rowsByDate.Values.Sum(r => r.QuantitiesByServiceItemId.GetValueOrDefault(s.ServiceItemId)));

        return new ValuationReportPivotDto
        {
            Services = services,
            Rows = rowsByDate.Values.OrderBy(r => r.Date).ToList(),
            ColumnTotals = columnTotals,
            GrandTotal = columnTotals.Values.Sum()
        };
    }

    private async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = await _storage.OpenReadAsync(filePath, cancellationToken);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }
}

public sealed class ValuationReportPhotoItem
{
    public string GuideNumber { get; set; } = string.Empty;

    public DateTime PickupDate { get; set; }

    public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
}
