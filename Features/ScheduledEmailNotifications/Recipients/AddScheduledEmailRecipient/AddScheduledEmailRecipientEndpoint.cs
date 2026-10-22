using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class AddScheduledEmailRecipientEndpoint
{
    public static void MapAddScheduledEmailRecipient(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:int}/recipients", async (
            int id,
            AddScheduledEmailRecipientRequest request,
            IValidator<AddScheduledEmailRecipientRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var configExists = await db.ScheduledEmailNotifications.AnyAsync(x => x.Id == id);
            if (!configExists)
            {
                return Results.NotFound("Configuración no encontrada.");
            }

            // Un mismo correo no puede repetirse dentro de TO, dentro de BCC, ni existir en ambos a la vez.
            var duplicate = await db.ScheduledEmailRecipients.AnyAsync(x =>
                x.ScheduledEmailNotificationId == id &&
                x.Email == request.Email);

            if (duplicate)
            {
                return Results.BadRequest(new[] { $"'{request.Email}' ya está agregado como destinatario (TO o BCC) de esta configuración." });
            }

            var recipient = new ScheduledEmailRecipient
            {
                ScheduledEmailNotificationId = id,
                Email = request.Email,
                RecipientType = request.RecipientType,
                CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"))
            };

            await db.ScheduledEmailRecipients.AddAsync(recipient);
            await db.SaveChangesAsync();

            var response = new ApiResponse<ScheduledEmailRecipientResponse>
            {
                Success = true,
                Message = "Destinatario agregado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = new ScheduledEmailRecipientResponse(recipient.Id, recipient.Email, recipient.RecipientType, recipient.CreatedAt)
            };

            return Results.Created($"/api/scheduled-email-notifications/{id}/recipients/{recipient.Id}", response);
        })
        .WithName("AddScheduledEmailRecipient")
        .Produces<ApiResponse<ScheduledEmailRecipientResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
