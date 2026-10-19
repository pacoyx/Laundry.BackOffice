public static class EmailAccountsEndpoints
{
    public static void MapEmailAccounts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/email-accounts").WithTags("EmailAccounts").RequirePermission(PermissionCodes.Settings);

        group.MapCreateEmailAccount();
        group.MapUpdateEmailAccount();
        group.MapDeleteEmailAccount();
        group.MapGetEmailAccounts();
        group.MapGetEmailAccount();
    }

    internal static EmailAccountResponse ToResponse(EmailAccount account) => new(
        account.Id, account.Operacion, account.Provider, account.Host, account.Port, account.UserName,
        account.EnableSsl, account.IsActive, !string.IsNullOrEmpty(account.Password), account.CreatedAt, account.UpdatedAt
    );
}
