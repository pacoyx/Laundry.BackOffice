using Microsoft.EntityFrameworkCore;

public static class UpdateInvoiceReminderSuppressionEndpoint
{
    public static void MapUpdateInvoiceReminderSuppression(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:long}/reminder-suppression", async (
            long id,
            UpdateInvoiceReminderSuppressionRequest request,
            RecepcionDbContext db) =>
        {
            var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id);
            if (invoice is null)
            {
                return Results.NotFound("Factura no encontrada.");
            }

            invoice.ReminderSuppressed = request.Suppressed;
            invoice.ReminderSuppressedReason = request.Suppressed ? request.Reason : null;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                Message = request.Suppressed
                    ? "Recordatorio silenciado para esta factura."
                    : "Recordatorio habilitado para esta factura.",
                StatusCode = StatusCodes.Status200OK,
                Data = "OK"
            });
        })
        .WithName("UpdateInvoiceReminderSuppression")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
