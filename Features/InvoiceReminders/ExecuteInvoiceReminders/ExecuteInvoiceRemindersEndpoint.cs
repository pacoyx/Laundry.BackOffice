using Microsoft.EntityFrameworkCore;

public static class ExecuteInvoiceRemindersEndpoint
{
    public static void MapExecuteInvoiceReminders(this IEndpointRouteBuilder app)
    {
        app.MapPost("/execute", async (RecepcionDbContext db, IOverdueInvoiceReminderService service) =>
        {
            var config = await db.ScheduledEmailNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == InvoiceReminderConstants.ConfigCode);

            if (config is null)
            {
                return Results.NotFound("No existe la configuración de recordatorio de facturas vencidas.");
            }

            await service.ExecuteAsync(config.Id);

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Recordatorios de facturas vencidas procesados.",
                StatusCode = StatusCodes.Status200OK,
                Data = "OK"
            });
        })
        .WithName("ExecuteInvoiceReminders")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
