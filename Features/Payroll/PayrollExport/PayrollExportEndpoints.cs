public static class PayrollExportEndpoints
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static RouteGroupBuilder MapPayrollExport(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}/payroll/excel", async (long id, IPayrollExportService service) =>
            ToFile(id, await service.GenerateExcelAsync(id), ExcelContentType))
        .WithName("ExportPayrollExcel")
        .Produces(StatusCodes.Status200OK, contentType: ExcelContentType)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        group.MapGet("/{id:long}/payroll/pdf", async (long id, IPayrollExportService service) =>
            ToFile(id, await service.GeneratePdfAsync(id), "application/pdf"))
        .WithName("ExportPayrollPdf")
        .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }

    private static IResult ToFile(long periodId, PayrollExportResult result, string contentType) => result.Status switch
    {
        PayrollExportStatus.PeriodNotFound => Results.NotFound(new ApiResponse<object>
        {
            Success = false,
            StatusCode = StatusCodes.Status404NotFound,
            Message = $"Periodo de planilla con ID {periodId} no encontrado."
        }),
        PayrollExportStatus.NoPayrolls => Results.BadRequest(new[] { "El periodo no tiene planillas generadas para exportar." }),
        _ => Results.File(result.Content!, contentType, result.FileName)
    };
}
