using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class SendTestInvoiceReminderEndpoint
{
    public static void MapSendTestInvoiceReminder(this IEndpointRouteBuilder app)
    {
        app.MapPost("/send-test", async (
            SendTestInvoiceReminderRequest request,
            IValidator<SendTestInvoiceReminderRequest> validator,
            RecepcionDbContext db,
            IOverdueInvoiceReminderService service) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var config = await db.ScheduledEmailNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == InvoiceReminderConstants.ConfigCode);

            if (config is null)
            {
                return Results.NotFound("No existe la configuración de recordatorio de facturas vencidas.");
            }

            try
            {
                await service.SendTestAsync(config.Id, request.CustomerId, request.TestEmail);

                return Results.Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = $"Correo de prueba enviado a {request.TestEmail}.",
                    StatusCode = StatusCodes.Status200OK,
                    Data = "OK"
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"No se pudo enviar el correo de prueba: {ex.Message}",
                    StatusCode = StatusCodes.Status200OK,
                    Data = string.Empty
                });
            }
        })
        .WithName("SendTestInvoiceReminder")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
