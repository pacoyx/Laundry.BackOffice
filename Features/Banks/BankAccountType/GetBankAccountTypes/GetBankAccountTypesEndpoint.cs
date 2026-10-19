using Microsoft.EntityFrameworkCore;

public static class GetBankAccountTypesEndpoint
{
    public static void MapGetBankAccountTypes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var types = await db.BankAccountTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<BankAccountTypeResponse>>
            {
                Success = true,
                Message = "Listado de tipos de cuenta.",
                StatusCode = StatusCodes.Status200OK,
                Data = types.Select(BankAccountTypeEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetBankAccountTypes")
        .Produces<ApiResponse<List<BankAccountTypeResponse>>>(StatusCodes.Status200OK);
    }
}
