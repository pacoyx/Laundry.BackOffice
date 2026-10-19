using Microsoft.EntityFrameworkCore;

public static class DeleteEmailAccountEndpoint
{
    public static void MapDeleteEmailAccount(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id:int}", async (int id, RecepcionDbContext db) =>
        {
            var account = await db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == id);
            if (account is null)
            {
                return Results.NotFound("Cuenta de correo no encontrada.");
            }

            db.EmailAccounts.Remove(account);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Cuenta de correo eliminada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { account.Id }
            });
        })
        .WithName("DeleteEmailAccount")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
