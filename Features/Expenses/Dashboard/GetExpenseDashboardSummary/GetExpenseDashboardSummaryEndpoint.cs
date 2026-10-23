using Microsoft.EntityFrameworkCore;

public static class GetExpenseDashboardSummaryEndpoint
{
    public static void MapGetExpenseDashboardSummary(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var response = await ComputeSummaryAsync(db);

            return Results.Ok(new ApiResponse<ExpenseDashboardSummaryResponse>
            {
                Success = true,
                Message = "Resumen de gastos del mes.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        });
    }

    // Extraido para reutilizarse tambien desde el Management Dashboard sin duplicar la logica.
    internal static async Task<ExpenseDashboardSummaryResponse> ComputeSummaryAsync(RecepcionDbContext db)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var administrative = await db.Expenses
            .AsNoTracking()
            .Where(x => x.IsActive && x.ExpenseDate >= monthStart && x.ExpenseDate < monthEnd)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var administrativeCount = await db.Expenses
            .AsNoTracking()
            .CountAsync(x => x.IsActive && x.ExpenseDate >= monthStart && x.ExpenseDate < monthEnd);

        var operational = await db.ExpenseBoxMains
            .AsNoTracking()
            .Where(x => x.FechaGasto >= monthStart && x.FechaGasto < monthEnd)
            .SumAsync(x => (decimal?)x.Importe) ?? 0;

        var operationalCount = await db.ExpenseBoxMains
            .AsNoTracking()
            .CountAsync(x => x.FechaGasto >= monthStart && x.FechaGasto < monthEnd);

        return new ExpenseDashboardSummaryResponse(
            administrative + operational,
            operational,
            administrative,
            administrativeCount + operationalCount);
    }
}
