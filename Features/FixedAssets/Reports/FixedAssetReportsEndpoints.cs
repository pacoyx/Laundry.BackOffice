using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

public static class FixedAssetReportsEndpoints
{
    public static void MapFixedAssetReports(this IEndpointRouteBuilder app)
    {
        // Reporte de historial (pantalla)
        app.MapGet("/reports/history", async (
            [AsParameters] FixedAssetHistoryReportFilter filter,
            RecepcionDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (InvalidRange(filter)) return RangeError();

            var assets = await FixedAssetHistoryReportBuilder.LoadAssetsAsync(db, filter, cancellationToken);
            var report = FixedAssetHistoryReportBuilder.Build(assets, filter);

            return Results.Ok(new ApiResponse<FixedAssetHistoryReportResponse>
            {
                Success = true,
                Message = "Reporte de historial de activos fijos.",
                StatusCode = StatusCodes.Status200OK,
                Data = report
            });
        })
        .WithName("GetFixedAssetHistoryReport")
        .Produces<ApiResponse<FixedAssetHistoryReportResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Reporte de historial (Excel)
        app.MapGet("/reports/history/excel", async (
            [AsParameters] FixedAssetHistoryReportFilter filter,
            RecepcionDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (InvalidRange(filter)) return RangeError();

            var assets = await FixedAssetHistoryReportBuilder.LoadAssetsAsync(db, filter, cancellationToken);
            var report = FixedAssetHistoryReportBuilder.Build(assets, filter);
            var bytes = FixedAssetHistoryExcelBuilder.Build(report, filter);

            return Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HistorialActivosFijos_{DateTime.Now:yyyyMMdd}.xlsx");
        })
        .WithName("GetFixedAssetHistoryReportExcel")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // Ficha PDF de un activo (historial completo, sin filtro de fechas)
        app.MapGet("/{id:long}/sheet/pdf", async (
            long id,
            RecepcionDbContext db,
            IFileStorageService storage,
            IImageCompressor compressor,
            ICompanyProfileService companyProfile,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var filter = new FixedAssetHistoryReportFilter(null, null, null, null, null, id);
            var assets = await FixedAssetHistoryReportBuilder.LoadAssetsAsync(db, filter, cancellationToken);
            var asset = assets.FirstOrDefault();
            if (asset is null)
            {
                return Results.NotFound("Activo fijo no encontrado.");
            }

            var report = FixedAssetHistoryReportBuilder.Build(assets, filter);

            byte[]? photo = null;
            if (!string.IsNullOrEmpty(asset.PhotoPath))
            {
                try
                {
                    await using var stream = await storage.OpenReadAsync(asset.PhotoPath, cancellationToken);
                    using var buffer = new MemoryStream();
                    await stream.CopyToAsync(buffer, cancellationToken);
                    photo = compressor.CompressToJpeg(buffer.ToArray(), 600, 75);
                }
                catch (Exception ex)
                {
                    // Si la foto no se puede leer (borrada del storage, etc.) la ficha sale igual, sin foto.
                    loggerFactory.CreateLogger("FixedAssetSheetPdf")
                        .LogWarning(ex, "No se pudo leer la foto del activo {AssetId} ({Path})", id, asset.PhotoPath);
                }
            }

            var company = await companyProfile.GetCompanyAsync(cancellationToken);
            var logo = await companyProfile.GetCompanyLogoBytesAsync(cancellationToken);

            var document = new FixedAssetSheetPdfDocument(
                asset, report.Assets[0], report.Events, photo, logo,
                company is null ? null : (string.IsNullOrWhiteSpace(company.NameComercial) ? company.NameCompany : company.NameComercial));

            return Results.File(document.GeneratePdf(), "application/pdf", $"FichaActivo_{asset.Code}.pdf");
        })
        .WithName("GetFixedAssetSheetPdf")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static bool InvalidRange(FixedAssetHistoryReportFilter filter) =>
        filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date;

    private static IResult RangeError() =>
        Results.BadRequest(new[] { "La fecha 'desde' no puede ser posterior a la fecha 'hasta'." });
}
