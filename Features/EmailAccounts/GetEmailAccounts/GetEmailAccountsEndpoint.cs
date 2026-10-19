using Microsoft.EntityFrameworkCore;

public static class GetEmailAccountsEndpoint
{
    public static void MapGetEmailAccounts(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var accounts = await db.EmailAccounts
                .AsNoTracking()
                .OrderBy(x => x.Operacion)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<EmailAccountResponse>>
            {
                Success = true,
                Message = "Listado de cuentas de correo.",
                StatusCode = StatusCodes.Status200OK,
                Data = accounts.Select(EmailAccountsEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetEmailAccounts")
        .Produces<ApiResponse<List<EmailAccountResponse>>>(StatusCodes.Status200OK);
    }
}
