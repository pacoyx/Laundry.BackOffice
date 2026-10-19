using Microsoft.EntityFrameworkCore;

public static class GetCompanyBankAccountEndpoint
{
    public static RouteGroupBuilder MapGetCompanyBankAccount(this RouteGroupBuilder group)
    {
        group.MapGet("/{companyId:int}/bank-accounts/{id:int}", async (int companyId, int id, RecepcionDbContext db) =>
        {
            var account = await db.CompanyBankAccounts
                .AsNoTracking()
                .Include(x => x.Bank)
                .Include(x => x.BankAccountType)
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            if (account is null)
            {
                return Results.NotFound(new ApiResponse<CompanyBankAccountResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Cuenta bancaria con ID {id} no encontrada para la empresa {companyId}."
                });
            }

            return Results.Ok(new ApiResponse<CompanyBankAccountResponse>
            {
                Success = true,
                Message = "Cuenta bancaria encontrada.",
                StatusCode = StatusCodes.Status200OK,
                Data = CompanyBankAccountEndpoints.ToResponse(account)
            });
        })
        .WithName("GetCompanyBankAccount")
        .Produces<ApiResponse<CompanyBankAccountResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
