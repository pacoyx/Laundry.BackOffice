using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class GetInvoiceReminderLogsEndpoint
{
    public static void MapGetInvoiceReminderLogs(this IEndpointRouteBuilder app)
    {
        app.MapGet("/logs", async (
            [FromQuery] int? customerId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] InvoiceReminderLogStatus? status,
            RecepcionDbContext db) =>
        {
            var query = db.InvoiceReminderLogs
                .AsNoTracking()
                .Include(x => x.Customer).ThenInclude(c => c!.BillingProfile)
                .Include(x => x.Details).ThenInclude(d => d.Invoice)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == customerId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.RunAt.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.RunAt.Date <= toDate.Value.Date);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var logs = await query.OrderByDescending(x => x.RunAt).ToListAsync();

            var response = new ApiResponse<List<InvoiceReminderLogResponse>>
            {
                Success = true,
                Message = "Historial de recordatorios de facturas vencidas.",
                StatusCode = StatusCodes.Status200OK,
                Data = logs.Select(InvoiceRemindersEndpoints.ToResponse).ToList()
            };

            return Results.Ok(response);
        })
        .WithName("GetInvoiceReminderLogs")
        .Produces<ApiResponse<List<InvoiceReminderLogResponse>>>(StatusCodes.Status200OK);
    }
}
