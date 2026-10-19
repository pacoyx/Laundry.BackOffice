using Microsoft.EntityFrameworkCore;

public static class GetBankAccountTypeLookupEndpoint
{
    public static void MapGetBankAccountTypeLookup(this IEndpointRouteBuilder app)
    {
        app.MapGet("/lookup", async (RecepcionDbContext db) =>
        {
            var types = await db.BankAccountTypes
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Results.Ok(types.Select(BankAccountTypeEndpoints.ToLookupResponse).ToList());
        })
        .WithName("GetBankAccountTypeLookup")
        .Produces<List<BankAccountTypeLookupResponse>>(StatusCodes.Status200OK);
    }
}
