using Microsoft.EntityFrameworkCore;

public static class GetScheduledEmailExecutionsEndpoint
{
    public static void MapGetScheduledEmailExecutions(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}/executions", async (int id, RecepcionDbContext db) =>
        {
            var exists = await db.ScheduledEmailNotifications.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            var executions = await db.ScheduledEmailExecutions
                .AsNoTracking()
                .Where(x => x.ScheduledEmailNotificationId == id)
                .OrderByDescending(x => x.ExecutedAt)
                .ToListAsync();

            var response = new ApiResponse<List<ScheduledEmailExecutionResponse>>
            {
                Success = true,
                Message = "Historial de ejecuciones.",
                StatusCode = StatusCodes.Status200OK,
                Data = executions.Select(ScheduledEmailNotificationsEndpoints.ToExecutionResponse).ToList()
            };

            return Results.Ok(response);
        })
        .WithName("GetScheduledEmailExecutions")
        .Produces<ApiResponse<List<ScheduledEmailExecutionResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
