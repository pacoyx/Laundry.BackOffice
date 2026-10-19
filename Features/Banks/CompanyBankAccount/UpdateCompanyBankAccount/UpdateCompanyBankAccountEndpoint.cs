using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateCompanyBankAccountEndpoint
{
    public static RouteGroupBuilder MapUpdateCompanyBankAccount(this RouteGroupBuilder group)
    {
        group.MapPut("/{companyId:int}/bank-accounts/{id:int}", async (
            int companyId,
            int id,
            UpdateCompanyBankAccountRequest request,
            IValidator<UpdateCompanyBankAccountRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var account = await db.CompanyBankAccounts.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
            if (account is null)
            {
                return Results.NotFound(new ApiResponse<CompanyBankAccountResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Cuenta bancaria con ID {id} no encontrada para la empresa {companyId}."
                });
            }

            var bank = await db.Banks.FirstOrDefaultAsync(x => x.Id == request.BankId);
            if (bank is null)
            {
                return Results.BadRequest(new[] { $"Banco con ID {request.BankId} no encontrado." });
            }

            var accountType = await db.BankAccountTypes.FirstOrDefaultAsync(x => x.Id == request.BankAccountTypeId);
            if (accountType is null)
            {
                return Results.BadRequest(new[] { $"Tipo de cuenta con ID {request.BankAccountTypeId} no encontrado." });
            }

            account.BankId = request.BankId;
            account.BankAccountTypeId = request.BankAccountTypeId;
            account.CurrencyId = request.CurrencyId;
            account.AccountNumber = request.AccountNumber;
            account.InterbankAccountNumber = request.InterbankAccountNumber;
            account.Description = request.Description;
            account.Notes = request.Notes;
            account.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            account.Bank = bank;
            account.BankAccountType = accountType;

            var response = new ApiResponse<CompanyBankAccountResponse>
            {
                Success = true,
                Message = "Cuenta bancaria actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = CompanyBankAccountEndpoints.ToResponse(account)
            };

            return Results.Ok(response);
        })
        .WithName("UpdateCompanyBankAccount")
        .Produces<ApiResponse<CompanyBankAccountResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
