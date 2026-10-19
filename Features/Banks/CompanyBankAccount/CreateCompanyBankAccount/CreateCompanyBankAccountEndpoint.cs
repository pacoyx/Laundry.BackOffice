using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateCompanyBankAccountEndpoint
{
    public static RouteGroupBuilder MapCreateCompanyBankAccount(this RouteGroupBuilder group)
    {
        group.MapPost("/{companyId:int}/bank-accounts", async (
            int companyId,
            CreateCompanyBankAccountRequest request,
            IValidator<CreateCompanyBankAccountRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var companyExists = await db.Companies.AnyAsync(x => x.Id == companyId);
            if (!companyExists)
            {
                return Results.NotFound(new ApiResponse<CompanyBankAccountResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Empresa con ID {companyId} no encontrada."
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

            var account = new CompanyBankAccount
            {
                CompanyId = companyId,
                BankId = request.BankId,
                BankAccountTypeId = request.BankAccountTypeId,
                CurrencyId = request.CurrencyId,
                AccountNumber = request.AccountNumber,
                InterbankAccountNumber = request.InterbankAccountNumber,
                Description = request.Description,
                Notes = request.Notes,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            await db.CompanyBankAccounts.AddAsync(account);
            await db.SaveChangesAsync();

            account.Bank = bank;
            account.BankAccountType = accountType;

            var response = new ApiResponse<CompanyBankAccountResponse>
            {
                Success = true,
                Message = "Cuenta bancaria registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = CompanyBankAccountEndpoints.ToResponse(account)
            };

            return Results.Created($"/api/companies/{companyId}/bank-accounts/{account.Id}", response);
        })
        .WithName("CreateCompanyBankAccount")
        .Produces<ApiResponse<CompanyBankAccountResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
