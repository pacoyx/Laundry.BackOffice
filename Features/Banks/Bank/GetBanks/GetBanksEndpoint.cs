using Microsoft.EntityFrameworkCore;

public static class GetBanksEndpoint
{
    public static void MapGetBanks(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (RecepcionDbContext db) =>
        {
            var banks = await db.Banks
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<BankResponse>>
            {
                Success = true,
                Message = "Listado de bancos.",
                StatusCode = StatusCodes.Status200OK,
                Data = banks.Select(BankEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetBanks")
        .Produces<ApiResponse<List<BankResponse>>>(StatusCodes.Status200OK);
    }
}
