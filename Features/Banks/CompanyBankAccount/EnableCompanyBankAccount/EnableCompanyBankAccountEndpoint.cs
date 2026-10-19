using Microsoft.EntityFrameworkCore;

public static class EnableCompanyBankAccountEndpoint
{
    public static RouteGroupBuilder MapEnableCompanyBankAccount(this RouteGroupBuilder group)
    {
        group.MapPatch("/{companyId:int}/bank-accounts/{id:int}/enable", async (int companyId, int id, RecepcionDbContext db) =>
        {
            var account = await db.CompanyBankAccounts.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
            if (account is null)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Cuenta bancaria con ID {id} no encontrada para la empresa {companyId}."
                });
            }

            account.IsEnabled = true;
            account.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<string>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Cuenta bancaria habilitada correctamente."
            });
        })
        .WithName("EnableCompanyBankAccount")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
