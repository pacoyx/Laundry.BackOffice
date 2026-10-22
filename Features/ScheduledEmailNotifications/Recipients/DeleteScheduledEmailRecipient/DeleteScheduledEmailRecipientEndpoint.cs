using Hangfire;
using Microsoft.EntityFrameworkCore;

public static class DeleteScheduledEmailRecipientEndpoint
{
    public static void MapDeleteScheduledEmailRecipient(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id:int}/recipients/{recipientId:int}", async (
            int id,
            int recipientId,
            RecepcionDbContext db,
            IRecurringJobManager recurringJobManager) =>
        {
            var config = await db.ScheduledEmailNotifications
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (config is null)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            var recipient = config.Recipients.FirstOrDefault(x => x.Id == recipientId);
            if (recipient is null)
            {
                return Results.NotFound("Destinatario no encontrado.");
            }

            db.ScheduledEmailRecipients.Remove(recipient);
            config.Recipients.Remove(recipient);

            // Si ya no queda ningun destinatario TO, se deshabilita para no dejar un job
            // agendado que fallaria en cada ejecucion por falta de destinatarios.
            if (config.IsEnabled && !config.Recipients.Any(r => r.RecipientType == ScheduledEmailRecipientType.To))
            {
                config.IsEnabled = false;
                config.UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
                recurringJobManager.RemoveIfExists(config.Code);
            }

            await db.SaveChangesAsync();

            var response = new ApiResponse<string>
            {
                Success = true,
                Message = "Destinatario eliminado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = "OK"
            };

            return Results.Ok(response);
        })
        .WithName("DeleteScheduledEmailRecipient")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
