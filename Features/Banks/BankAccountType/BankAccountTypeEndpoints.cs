public static class BankAccountTypeEndpoints
{
    public static void MapBankAccountTypes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bank-account-types").WithTags("BankAccountTypes").RequirePermission(PermissionCodes.Settings);

        group.MapCreateBankAccountType();
        group.MapUpdateBankAccountType();
        group.MapGetBankAccountTypes();
        group.MapGetBankAccountType();
        group.MapGetBankAccountTypeLookup();
    }

    internal static BankAccountTypeResponse ToResponse(BankAccountType type) => new(
        type.Id, type.Name, type.IsActive, type.CreatedAt, type.UpdatedAt
    );

    internal static BankAccountTypeLookupResponse ToLookupResponse(BankAccountType type) => new(type.Id, type.Name);
}
