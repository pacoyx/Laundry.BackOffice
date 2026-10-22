using Microsoft.EntityFrameworkCore;

public static class ExecuteScheduledEmailNotificationEndpoint
{
    public static void MapExecuteScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/execute", async (int id, RecepcionDbContext db, IScheduledEmailService scheduledEmailService) =>
        {
            var exists = await db.ScheduledEmailNotifications.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            var execution = await scheduledEmailService.ExecuteAsync(id);
            var success = execution.Status == ScheduledEmailExecutionStatus.Success;

            var response = new ApiResponse<ScheduledEmailExecutionResponse>
            {
                Success = success,
                Message = success ? "Correo enviado correctamente." : "No se pudo enviar el correo.",
                StatusCode = StatusCodes.Status200OK,
                Data = ScheduledEmailNotificationsEndpoints.ToExecutionResponse(execution)
            };

            return Results.Ok(response);
        })
        .WithName("ExecuteScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailExecutionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
