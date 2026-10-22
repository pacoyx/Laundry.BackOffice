using Microsoft.EntityFrameworkCore;

public static class PreviewInvoiceReminderEndpoint
{
    public static void MapPreviewInvoiceReminder(this IEndpointRouteBuilder app)
    {
        app.MapPost("/preview/{customerId:int}", async (int customerId, RecepcionDbContext db, IOverdueInvoiceReminderService service) =>
        {
            var config = await db.ScheduledEmailNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == InvoiceReminderConstants.ConfigCode);

            if (config is null)
            {
                return Results.NotFound("No existe la configuración de recordatorio de facturas vencidas.");
            }

            try
            {
                var (subject, body) = await service.PreviewAsync(config.Id, customerId);

                return Results.Ok(new ApiResponse<InvoiceReminderPreviewResponse>
                {
                    Success = true,
                    Message = "Vista previa generada correctamente.",
                    StatusCode = StatusCodes.Status200OK,
                    Data = new InvoiceReminderPreviewResponse(subject, body)
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new[] { ex.Message });
            }
        })
        .WithName("PreviewInvoiceReminder")
        .Produces<ApiResponse<InvoiceReminderPreviewResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
