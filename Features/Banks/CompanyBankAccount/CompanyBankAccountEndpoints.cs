public static class CompanyBankAccountEndpoints
{
    public static void MapCompanyBankAccounts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies").WithTags("Finance - Company Bank Accounts").RequirePermission(PermissionCodes.Settings);

        group.MapCreateCompanyBankAccount();
        group.MapUpdateCompanyBankAccount();
        group.MapGetCompanyBankAccounts();
        group.MapGetCompanyBankAccount();
        group.MapEnableCompanyBankAccount();
        group.MapDisableCompanyBankAccount();
    }

    internal static CompanyBankAccountResponse ToResponse(CompanyBankAccount account) => new(
        account.Id,
        account.CompanyId,
        account.BankId,
        account.Bank!.Name,
        account.BankAccountTypeId,
        account.BankAccountType!.Name,
        account.CurrencyId,
        account.AccountNumber,
        account.InterbankAccountNumber,
        account.Description,
        account.IsEnabled,
        account.Notes,
        account.CreatedAt,
        account.UpdatedAt
    );
}
