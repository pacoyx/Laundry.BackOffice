using Microsoft.EntityFrameworkCore;

public static class GetEmailAccountEndpoint
{
    public static void MapGetEmailAccount(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}", async (int id, RecepcionDbContext db) =>
        {
            var account = await db.EmailAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (account is null)
            {
                return Results.NotFound("Cuenta de correo no encontrada.");
            }

            return Results.Ok(new ApiResponse<EmailAccountResponse>
            {
                Success = true,
                Message = "Cuenta de correo encontrada.",
                StatusCode = StatusCodes.Status200OK,
                Data = EmailAccountsEndpoints.ToResponse(account)
            });
        })
        .WithName("GetEmailAccount")
        .Produces<ApiResponse<EmailAccountResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
