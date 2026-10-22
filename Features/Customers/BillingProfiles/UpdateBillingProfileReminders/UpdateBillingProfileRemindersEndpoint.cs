using Microsoft.EntityFrameworkCore;

public static class UpdateBillingProfileRemindersEndpoint
{
    public static void MapUpdateBillingProfileReminders(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}/billing-profile/reminders", async (
            int id,
            UpdateBillingProfileRemindersRequest request,
            RecepcionDbContext db) =>
        {
            var profile = await db.CustomerBillingProfiles.FirstOrDefaultAsync(x => x.CustomerId == id);
            if (profile is null)
            {
                return Results.NotFound("El cliente no tiene un perfil de facturación registrado.");
            }

            profile.RemindersEnabled = request.RemindersEnabled;
            profile.RemindersSuppressedReason = request.RemindersEnabled ? null : request.Reason;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = request.RemindersEnabled
                    ? "Recordatorios de facturas vencidas habilitados para este cliente."
                    : "Recordatorios de facturas vencidas silenciados para este cliente.",
                StatusCode = StatusCodes.Status200OK,
                Data = "OK"
            });
        })
        .WithName("UpdateBillingProfileReminders")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
