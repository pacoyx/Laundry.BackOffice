using Microsoft.EntityFrameworkCore;

public static class GetScheduledEmailNotificationsEndpoint
{
    public static void MapGetScheduledEmailNotifications(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var configs = await db.ScheduledEmailNotifications
                .AsNoTracking()
                .Include(x => x.Recipients)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Config = x,
                    LastExecutionAt = x.Executions.Max(e => (DateTime?)e.ExecutedAt),
                    ExecutionCount = x.Executions.Count()
                })
                .ToListAsync();

            var response = new ApiResponse<List<ScheduledEmailNotificationSummaryResponse>>
            {
                Success = true,
                Message = "Listado de correos programados.",
                StatusCode = StatusCodes.Status200OK,
                Data = configs
                    .Select(x => ScheduledEmailNotificationsEndpoints.ToSummaryResponse(x.Config, x.LastExecutionAt, x.ExecutionCount))
                    .ToList()
            };

            return Results.Ok(response);
        })
        .WithName("GetScheduledEmailNotifications")
        .Produces<ApiResponse<List<ScheduledEmailNotificationSummaryResponse>>>(StatusCodes.Status200OK);
    }
}
