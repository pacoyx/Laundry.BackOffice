using System.Security.Claims;
using FluentValidation;
using Hangfire;
using Microsoft.EntityFrameworkCore;

public static class UpdateScheduledEmailNotificationEndpoint
{
    public static void MapUpdateScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}", async (
            int id,
            UpdateScheduledEmailNotificationRequest request,
            IValidator<UpdateScheduledEmailNotificationRequest> validator,
            RecepcionDbContext db,
            IRecurringJobManager recurringJobManager,
            ClaimsPrincipal principal) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var config = await db.ScheduledEmailNotifications
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (config is null)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            config.Name = request.Name;
            config.CronExpression = CronScheduleHelper.BuildCronExpression(request.Hour, request.Minute, request.DaysOfWeek);
            config.SubjectTemplate = request.SubjectTemplate;
            config.BodyTemplate = request.BodyTemplate;
            config.DriverName = request.DriverName;
            config.DriverDocument = request.DriverDocument;
            config.SignatureName = request.SignatureName ?? string.Empty;
            config.SignaturePosition = request.SignaturePosition ?? string.Empty;
            config.SignaturePhone = request.SignaturePhone ?? string.Empty;
            config.UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            config.UpdatedBy = principal.Identity?.Name;

            await db.SaveChangesAsync();

            // Si esta habilitada, re-registra el job con el nuevo cron (el horario pudo cambiar).
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
                Message = "Configuración de correo programado actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = ScheduledEmailNotificationsEndpoints.ToResponse(config, stats?.LastExecutionAt, stats?.ExecutionCount ?? 0)
            };

            return Results.Ok(response);
        })
        .WithName("UpdateScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailNotificationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
