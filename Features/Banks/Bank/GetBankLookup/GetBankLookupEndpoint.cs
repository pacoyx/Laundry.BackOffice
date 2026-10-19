using Microsoft.EntityFrameworkCore;

public static class GetBankLookupEndpoint
{
    public static void MapGetBankLookup(this IEndpointRouteBuilder app)
    {
        app.MapGet("/lookup", async (RecepcionDbContext db) =>
        {
            var banks = await db.Banks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Results.Ok(banks.Select(BankEndpoints.ToLookupResponse).ToList());
        })
        .WithName("GetBankLookup")
        .Produces<List<BankLookupResponse>>(StatusCodes.Status200OK);
    }
}
