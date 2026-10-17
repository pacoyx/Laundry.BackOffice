public interface IValuationReportService
{
    Task<ValuationReportResponse> GetDataAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default);

    Task<byte[]> GeneratePhotosPdfAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateSummaryPdfAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateSummaryExcelAsync(ValuationReportFilter filter, CancellationToken cancellationToken = default);
}
