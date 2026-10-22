using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class SendTestScheduledEmailNotificationEndpoint
{
    public static void MapSendTestScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/send-test", async (
            int id,
            SendTestScheduledEmailNotificationRequest request,
            IValidator<SendTestScheduledEmailNotificationRequest> validator,
            RecepcionDbContext db,
            IScheduledEmailService scheduledEmailService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var exists = await db.ScheduledEmailNotifications.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            try
            {
                await scheduledEmailService.SendTestAsync(id, request.TestEmail);

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
        .WithName("SendTestScheduledEmailNotification")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
