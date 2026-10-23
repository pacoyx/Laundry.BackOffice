using Microsoft.EntityFrameworkCore;

public static class GetHrDashboardSummaryEndpoint
{
    public static void MapGetHrDashboardSummary(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/hr-dashboard", async (RecepcionDbContext db) =>
        {
            var response = await ComputeSummaryAsync(db);

            return Results.Ok(new ApiResponse<HrDashboardSummaryResponseDto>
            {
                Success = true,
                Data = response,
                StatusCode = StatusCodes.Status200OK,
                Message = "Resumen del dashboard de RRHH."
            });
        })
        .WithName("GetHrDashboardSummary")
        .WithTags("HR - Dashboard")
        .Produces<ApiResponse<HrDashboardSummaryResponseDto>>(StatusCodes.Status200OK)
        .RequireAuthorization();
    }

    // Extraido para reutilizarse tambien desde el Management Dashboard sin duplicar la logica.
    internal static async Task<HrDashboardSummaryResponseDto> ComputeSummaryAsync(RecepcionDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var activeEmployees = await db.Employees.AsNoTracking().CountAsync(x => x.IsActive);

        var monthlyPayrollCost = await db.Employees.AsNoTracking()
            .Where(x => x.IsActive)
            .SumAsync(x => (decimal?)x.BaseSalary) ?? 0;

        var pendingLoans = await db.EmployeeLoans.AsNoTracking()
            .Where(x => !x.IsClosed)
            .SumAsync(x => (decimal?)x.Balance) ?? 0;

        var extraHoursMonth = await db.EmployeeExtraHours.AsNoTracking()
            .Where(x => x.Approved && x.Date >= monthStart && x.Date <= monthEnd)
            .SumAsync(x => (decimal?)x.Hours) ?? 0;

        return new HrDashboardSummaryResponseDto
        {
            ActiveEmployees = activeEmployees,
            MonthlyPayrollCost = monthlyPayrollCost,
            PendingLoans = pendingLoans,
            ExtraHoursMonth = extraHoursMonth
        };
    }
}
