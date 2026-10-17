using FluentValidation;
using Microsoft.AspNetCore.Mvc;

public static class ValuationReportEndpoints
{
    public static void MapValuationReport(this IEndpointRouteBuilder app)
    {
        app.MapGet("/valuation-report", async (
            [FromQuery] int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            IValidator<ValuationReportFilter> validator,
            IValuationReportService reportService,
            CancellationToken cancellationToken) =>
        {
            var (filter, error) = await ValidateFilterAsync(customerId, fromDate, toDate, validator);
            if (error is not null)
            {
                return error;
            }

            var data = await reportService.GetDataAsync(filter!, cancellationToken);

            return Results.Ok(new ApiResponse<ValuationReportResponse>
            {
                Success = true,
                Message = "Valorización del mes.",
                StatusCode = StatusCodes.Status200OK,
                Data = data
            });
        })
        .WithName("GetValuationReport")
        .Produces<ApiResponse<ValuationReportResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/valuation-report/photos-pdf", async (
            [FromQuery] int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            IValidator<ValuationReportFilter> validator,
            IValuationReportService reportService,
            CancellationToken cancellationToken) =>
        {
            var (filter, error) = await ValidateFilterAsync(customerId, fromDate, toDate, validator);
            if (error is not null)
            {
                return error;
            }

            var pdfBytes = await reportService.GeneratePhotosPdfAsync(filter!, cancellationToken);
            var fileName = $"Valorizacion_Fotos_{customerId}_{DateTime.Now:yyyyMMdd}.pdf";

            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithName("GetValuationReportPhotosPdf")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/valuation-report/summary-pdf", async (
            [FromQuery] int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            IValidator<ValuationReportFilter> validator,
            IValuationReportService reportService,
            CancellationToken cancellationToken) =>
        {
            var (filter, error) = await ValidateFilterAsync(customerId, fromDate, toDate, validator);
            if (error is not null)
            {
                return error;
            }

            var pdfBytes = await reportService.GenerateSummaryPdfAsync(filter!, cancellationToken);
            var fileName = $"Valorizacion_Resumen_{customerId}_{DateTime.Now:yyyyMMdd}.pdf";

            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithName("GetValuationReportSummaryPdf")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/valuation-report/summary-excel", async (
            [FromQuery] int customerId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            IValidator<ValuationReportFilter> validator,
            IValuationReportService reportService,
            CancellationToken cancellationToken) =>
        {
            var (filter, error) = await ValidateFilterAsync(customerId, fromDate, toDate, validator);
            if (error is not null)
            {
                return error;
            }

            var excelBytes = await reportService.GenerateSummaryExcelAsync(filter!, cancellationToken);
            var fileName = $"Valorizacion_Resumen_{customerId}_{DateTime.Now:yyyyMMdd}.xlsx";

            return Results.File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
        .WithName("GetValuationReportSummaryExcel")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<(ValuationReportFilter? Filter, IResult? Error)> ValidateFilterAsync(
        int customerId,
        DateTime fromDate,
        DateTime toDate,
        IValidator<ValuationReportFilter> validator)
    {
        var filter = new ValuationReportFilter
        {
            CustomerId = customerId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var validation = await validator.ValidateAsync(filter);
        if (!validation.IsValid)
        {
            return (null, Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage)));
        }

        return (filter, null);
    }
}
