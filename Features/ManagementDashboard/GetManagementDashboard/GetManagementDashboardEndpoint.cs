using Microsoft.EntityFrameworkCore;

public static class GetManagementDashboardEndpoint
{
    public static void MapGetManagementDashboard(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            // Reutiliza la logica ya existente de cada dashboard de modulo, sin duplicarla.
            var invoicing = await GetInvoiceDashboardSummaryEndpoint.ComputeSummaryAsync(db);
            var expenses = await GetExpenseDashboardSummaryEndpoint.ComputeSummaryAsync(db);
            var purchases = await GetPurchaseDashboardSummaryEndpoint.ComputeSummaryAsync(db);
            var hr = await GetHrDashboardSummaryEndpoint.ComputeSummaryAsync(db);

            var collections = await ComputeCollectionsSummaryAsync(db, monthStart, monthEnd);
            var operations = await ComputeOperationsSummaryAsync(db, monthStart, monthEnd);
            var (deliveries, receptionIncome) = await ComputeReceptionSummariesAsync(db, monthStart, monthEnd);

            var businessSummary = ComputeBusinessSummary(invoicing, expenses, purchases, hr, receptionIncome);

            var response = new ManagementDashboardResponse(
                invoicing, collections, operations, deliveries, receptionIncome,
                expenses, purchases, hr, businessSummary
            );

            return Results.Ok(new ApiResponse<ManagementDashboardResponse>
            {
                Success = true,
                Message = "Resumen del dashboard de management.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("GetManagementDashboard")
        .Produces<ApiResponse<ManagementDashboardResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<CollectionsSummaryResponse> ComputeCollectionsSummaryAsync(RecepcionDbContext db, DateTime monthStart, DateTime monthEnd)
    {
        var statusCounts = await db.InvoiceReminderLogs
            .AsNoTracking()
            .Where(x => x.RunAt >= monthStart && x.RunAt < monthEnd)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var lastRunAt = await db.InvoiceReminderLogs.AsNoTracking().MaxAsync(x => (DateTime?)x.RunAt);

        return new CollectionsSummaryResponse(
            statusCounts.FirstOrDefault(x => x.Status == InvoiceReminderLogStatus.Sent)?.Count ?? 0,
            statusCounts.FirstOrDefault(x => x.Status == InvoiceReminderLogStatus.Failed)?.Count ?? 0,
            statusCounts.FirstOrDefault(x => x.Status == InvoiceReminderLogStatus.Skipped)?.Count ?? 0,
            lastRunAt
        );
    }

    private static async Task<OperationsSummaryResponse> ComputeOperationsSummaryAsync(RecepcionDbContext db, DateTime monthStart, DateTime monthEnd)
    {
        var pickupGuides = await db.PickupGuides
            .AsNoTracking()
            .Where(x => x.PickupDate >= monthStart && x.PickupDate < monthEnd)
            .Select(x => new { x.Status, x.TotalAmount })
            .ToListAsync();

        return new OperationsSummaryResponse(
            pickupGuides.Count,
            pickupGuides.Count(x => x.Status == PickupGuideStatus.Draft || x.Status == PickupGuideStatus.Collected),
            pickupGuides.Count(x => x.Status == PickupGuideStatus.Processed || x.Status == PickupGuideStatus.Invoiced),
            pickupGuides.Where(x => x.Status != PickupGuideStatus.Cancelled).Sum(x => x.TotalAmount)
        );
    }

    private static async Task<(DeliverySummaryResponse Deliveries, ReceptionIncomeSummaryResponse ReceptionIncome)> ComputeReceptionSummariesAsync(RecepcionDbContext db, DateTime monthStart, DateTime monthEnd)
    {
        var workGuides = await db.WorkGuideMains
            .AsNoTracking()
            .Where(x => x.FechaOperacion >= monthStart && x.FechaOperacion < monthEnd && x.EstadoRegistro == "A")
            .Select(x => new { x.TipoRecepcion, x.EstadoSituacion, x.Total, x.Acuenta, x.Saldo })
            .ToListAsync();

        var deliveryGuides = workGuides.Where(x => x.TipoRecepcion == "D").ToList();
        var receptionGuides = workGuides.Where(x => x.TipoRecepcion == "R").ToList();

        var deliveries = new DeliverySummaryResponse(
            deliveryGuides.Count,
            deliveryGuides.Sum(x => x.Total),
            receptionGuides.Count,
            receptionGuides.Sum(x => x.Total),
            deliveryGuides.Count(x => x.EstadoSituacion == "P"),
            // "Entregado" y "Devuelto" se consideran ambos como ya resueltos/completados.
            deliveryGuides.Count(x => x.EstadoSituacion == "E" || x.EstadoSituacion == "D")
        );

        var paymentRows = await db.CashBoxDetails
            .AsNoTracking()
            .Where(x => x.FechaComprobante >= monthStart && x.FechaComprobante < monthEnd && x.EstadoRegistro == "A")
            .Select(x => new { x.TipoPago, Amount = x.Importe + x.Adelanto })
            .ToListAsync();

        var paymentMethodBreakdown = paymentRows
            .GroupBy(x => x.TipoPago)
            .Select(g => new PaymentMethodBreakdownItem(
                ManagementDashboardEndpoints.ResolvePaymentMethodLabel(g.Key),
                g.Sum(x => x.Amount),
                g.Count()))
            .OrderByDescending(x => x.Amount)
            .ToList();

        var receptionIncome = new ReceptionIncomeSummaryResponse(
            workGuides.Sum(x => x.Total),
            workGuides.Sum(x => x.Acuenta),
            workGuides.Sum(x => x.Saldo),
            workGuides.Count,
            paymentMethodBreakdown
        );

        return (deliveries, receptionIncome);
    }

    private static BusinessSummaryResponse ComputeBusinessSummary(
        InvoiceDashboardResponse invoicing,
        ExpenseDashboardSummaryResponse expenses,
        PurchaseDashboardSummaryResponseDto purchases,
        HrDashboardSummaryResponseDto hr,
        ReceptionIncomeSummaryResponse receptionIncome)
    {
        var totalIncome = invoicing.MonthInvoiced + receptionIncome.MonthIncome;
        var totalExpenses = expenses.CurrentMonthExpenses + purchases.MonthPurchases + hr.MonthlyPayrollCost;
        var netResult = totalIncome - totalExpenses;
        var marginPercent = totalIncome > 0 ? Math.Round(netResult / totalIncome * 100, 1) : 0;

        var expenseBreakdown = new List<ExpenseBreakdownItem>
        {
            new("Planilla", hr.MonthlyPayrollCost, PercentOf(hr.MonthlyPayrollCost, totalExpenses)),
            new("Gastos operativos/administrativos", expenses.CurrentMonthExpenses, PercentOf(expenses.CurrentMonthExpenses, totalExpenses)),
            new("Compras", purchases.MonthPurchases, PercentOf(purchases.MonthPurchases, totalExpenses))
        }.OrderByDescending(x => x.Amount).ToList();

        var totalPendingToCollect = invoicing.AccountsReceivable + receptionIncome.PendingBalance;
        var pendingCollectionRatio = totalIncome > 0 ? Math.Round(totalPendingToCollect / totalIncome * 100, 1) : 0;

        return new BusinessSummaryResponse(
            totalIncome, totalExpenses, netResult, marginPercent, netResult >= 0,
            expenseBreakdown, totalPendingToCollect, pendingCollectionRatio
        );
    }

    private static decimal PercentOf(decimal part, decimal total) =>
        total > 0 ? Math.Round(part / total * 100, 1) : 0;
}
