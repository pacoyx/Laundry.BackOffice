using Microsoft.EntityFrameworkCore;

public static class GetCompanyBankAccountsEndpoint
{
    public static RouteGroupBuilder MapGetCompanyBankAccounts(this RouteGroupBuilder group)
    {
        group.MapGet("/{companyId:int}/bank-accounts", async (int companyId, RecepcionDbContext db) =>
        {
            var companyExists = await db.Companies.AnyAsync(x => x.Id == companyId);
            if (!companyExists)
            {
                return Results.NotFound(new ApiResponse<List<CompanyBankAccountResponse>>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Empresa con ID {companyId} no encontrada."
                });
            }

            var accounts = await db.CompanyBankAccounts
                .AsNoTracking()
                .Include(x => x.Bank)
                .Include(x => x.BankAccountType)
                .Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.Bank!.Name)
                .ThenBy(x => x.AccountNumber)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<CompanyBankAccountResponse>>
            {
                Success = true,
                Message = "Listado de cuentas bancarias de la empresa.",
                StatusCode = StatusCodes.Status200OK,
                Data = accounts.Select(CompanyBankAccountEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetCompanyBankAccounts")
        .Produces<ApiResponse<List<CompanyBankAccountResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
