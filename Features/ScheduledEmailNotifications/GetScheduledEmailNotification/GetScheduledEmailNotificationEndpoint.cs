using Microsoft.EntityFrameworkCore;

public static class GetScheduledEmailNotificationEndpoint
{
    public static void MapGetScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}", async (int id, RecepcionDbContext db) =>
        {
            var config = await db.ScheduledEmailNotifications
                .AsNoTracking()
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (config is null)
            {
                return Results.NotFound();
            }

            var stats = await db.ScheduledEmailExecutions
                .AsNoTracking()
                .Where(x => x.ScheduledEmailNotificationId == id)
                .GroupBy(x => x.ScheduledEmailNotificationId)
                .Select(g => new { LastExecutionAt = (DateTime?)g.Max(e => e.ExecutedAt), ExecutionCount = g.Count() })
                .FirstOrDefaultAsync();

            var response = new ApiResponse<ScheduledEmailNotificationResponse>
            {
                Success = true,
                Message = "Configuración de correo programado obtenida correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = ScheduledEmailNotificationsEndpoints.ToResponse(config, stats?.LastExecutionAt, stats?.ExecutionCount ?? 0)
            };

            return Results.Ok(response);
        })
        .WithName("GetScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailNotificationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
