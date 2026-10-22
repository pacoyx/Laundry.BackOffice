using System.Security.Claims;
using Hangfire;
using Microsoft.EntityFrameworkCore;

public static class DisableScheduledEmailNotificationEndpoint
{
    public static void MapDisableScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/disable", async (int id, RecepcionDbContext db, IRecurringJobManager recurringJobManager, ClaimsPrincipal principal) =>
        {
            var config = await db.ScheduledEmailNotifications
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (config is null)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            config.IsEnabled = false;
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
                Message = "Configuración deshabilitada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = ScheduledEmailNotificationsEndpoints.ToResponse(config, stats?.LastExecutionAt, stats?.ExecutionCount ?? 0)
            };

            return Results.Ok(response);
        })
        .WithName("DisableScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailNotificationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
