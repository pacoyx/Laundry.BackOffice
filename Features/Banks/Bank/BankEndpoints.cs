public static class BankEndpoints
{
    public static void MapBanks(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/banks").WithTags("Banks").RequirePermission(PermissionCodes.Settings);

        group.MapCreateBank();
        group.MapUpdateBank();
        group.MapGetBanks();
        group.MapGetBank();
        group.MapGetBankLookup();
    }

    internal static BankResponse ToResponse(Bank bank) => new(
        bank.Id, bank.Name, bank.ShortName, bank.IsActive, bank.CreatedAt, bank.UpdatedAt
    );

    internal static BankLookupResponse ToLookupResponse(Bank bank) => new(bank.Id, bank.Name);
}
