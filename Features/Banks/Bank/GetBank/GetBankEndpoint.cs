using Microsoft.EntityFrameworkCore;

public static class GetBankEndpoint
{
    public static void MapGetBank(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}", async (int id, RecepcionDbContext db) =>
        {
            var bank = await db.Banks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (bank is null)
            {
                return Results.NotFound("Banco no encontrado.");
            }

            return Results.Ok(new ApiResponse<BankResponse>
            {
                Success = true,
                Message = "Banco encontrado.",
                StatusCode = StatusCodes.Status200OK,
                Data = BankEndpoints.ToResponse(bank)
            });
        })
        .WithName("GetBank")
        .Produces<ApiResponse<BankResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
