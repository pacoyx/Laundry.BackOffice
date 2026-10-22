using Microsoft.EntityFrameworkCore;

public static class PreviewScheduledEmailNotificationEndpoint
{
    public static void MapPreviewScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/preview", async (int id, RecepcionDbContext db, IScheduledEmailService scheduledEmailService) =>
        {
            var exists = await db.ScheduledEmailNotifications.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            var (subject, body) = await scheduledEmailService.PreviewAsync(id);

            var response = new ApiResponse<ScheduledEmailPreviewResponse>
            {
                Success = true,
                Message = "Vista previa generada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new ScheduledEmailPreviewResponse(subject, body)
            };

            return Results.Ok(response);
        })
        .WithName("PreviewScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailPreviewResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
