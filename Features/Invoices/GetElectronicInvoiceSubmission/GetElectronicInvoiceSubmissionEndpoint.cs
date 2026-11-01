using Microsoft.EntityFrameworkCore;

public static class GetElectronicInvoiceSubmissionEndpoint
{
    public static void MapGetElectronicInvoiceSubmission(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:long}/electronic-submission", async (long id, RecepcionDbContext db) =>
        {
            var submission = await db.ElectronicInvoiceSubmissions
                .AsNoTracking()
                .Where(x => x.InvoiceId == id)
                .OrderByDescending(x => x.SentAt)
                .FirstOrDefaultAsync();

            if (submission is null)
            {
                return Results.NotFound();
            }

            var response = new ElectronicInvoiceSubmissionResponse(
                submission.Id,
                submission.InvoiceId,
                submission.Provider,
                submission.SentAt,
                submission.Success,
                submission.AcceptedBySunat,
                submission.SunatDescription,
                submission.SunatResponseCode,
                submission.DocumentUrl,
                submission.ErrorMessage
            );

            return Results.Ok(new ApiResponse<ElectronicInvoiceSubmissionResponse>
            {
                Success = true,
                Message = "Envio de facturacion electronica encontrado.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .RequirePermission(PermissionCodes.ElectronicInvoicing);
    }
}
