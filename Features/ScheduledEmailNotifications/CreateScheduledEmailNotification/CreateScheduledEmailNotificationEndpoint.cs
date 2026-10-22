using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateScheduledEmailNotificationEndpoint
{
    public static void MapCreateScheduledEmailNotification(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateScheduledEmailNotificationRequest request,
            IValidator<CreateScheduledEmailNotificationRequest> validator,
            RecepcionDbContext db,
            ClaimsPrincipal principal) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var codeInUse = await db.ScheduledEmailNotifications.AnyAsync(x => x.Code == request.Code);
            if (codeInUse)
            {
                return Results.BadRequest(new[] { $"Ya existe una configuración con el código '{request.Code}'." });
            }

            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            // Se crea siempre deshabilitada: el admin debe probarla (correo de prueba) y
            // habilitarla explicitamente una vez que tenga al menos un destinatario TO.
            var config = new ScheduledEmailNotification
            {
                Code = request.Code,
                Name = request.Name,
                CronExpression = CronScheduleHelper.BuildCronExpression(request.Hour, request.Minute, request.DaysOfWeek),
                SubjectTemplate = request.SubjectTemplate,
                BodyTemplate = request.BodyTemplate,
                DriverName = request.DriverName,
                DriverDocument = request.DriverDocument,
                SignatureName = request.SignatureName ?? string.Empty,
                SignaturePosition = request.SignaturePosition ?? string.Empty,
                SignaturePhone = request.SignaturePhone ?? string.Empty,
                IsEnabled = false,
                CreatedAt = now,
                CreatedBy = principal.Identity?.Name
            };

            await db.ScheduledEmailNotifications.AddAsync(config);
            await db.SaveChangesAsync();

            var response = new ApiResponse<ScheduledEmailNotificationResponse>
            {
                Success = true,
                Message = "Configuración de correo programado creada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = ScheduledEmailNotificationsEndpoints.ToResponse(config, lastExecutionAt: null, executionCount: 0)
            };

            return Results.Created($"/api/scheduled-email-notifications/{config.Id}", response);
        })
        .WithName("CreateScheduledEmailNotification")
        .Produces<ApiResponse<ScheduledEmailNotificationResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
