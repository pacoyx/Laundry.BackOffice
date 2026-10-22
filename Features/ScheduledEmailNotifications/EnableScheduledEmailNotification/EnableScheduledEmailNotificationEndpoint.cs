using System.Security.Claims;
using Hangfire;
using Microsoft.EntityFrameworkCore;

public static class EnableScheduledEmailNotificationEndpoint
{
    public static void MapEnableScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/enable", async (int id, RecepcionDbContext db, IRecurringJobManager recurringJobManager, ClaimsPrincipal principal) =>
        {
            var config = await db.ScheduledEmailNotifications
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (config is null)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            // En modo Dynamic los destinatarios se resuelven en tiempo de ejecucion
            // (ej. por cliente), no hay nada que validar aqui en ScheduledEmailRecipients.
            if (config.RecipientMode == ScheduledEmailRecipientMode.Fixed)
            {
                var hasToRecipient = config.Recipients.Any(r => r.RecipientType == ScheduledEmailRecipientType.To);
                if (!hasToRecipient)
                {
                    return Results.BadRequest(new[] { "No se puede habilitar: debe tener al menos un destinatario TO configurado." });
                }
            }

            config.IsEnabled = true;
            config.UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            config.UpdatedBy = principal.Identity?.Name;
            await db.SaveChangesAsync();

            recurringJobManager.SyncJob(config);

            var stats = await db.ScheduledEmailExecutions
                .AsNoTracking()
                .Where(x => x.ScheduledEmailNotificationId == id)
                .GroupBy(x => x.ScheduledEmailNotificationId)
                .Select(g => new { LastExecutionAt = (DateTime?)g.Max(e => e.ExecutedAt), ExecutionCount = g.Count() })
                .FirstOrDefaultAsync();

            var response = new ApiResponse<ScheduledEmailNotificationResponse>
            {
                Success = true,
                Message = "Configuración habilitada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = ScheduledEmailNotificationsEndpoints.ToResponse(config, stats?.LastExecutionAt, stats?.ExecutionCount ?? 0)
            };

            return Results.Ok(response);
        })
        .WithName("EnableScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailNotificationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
