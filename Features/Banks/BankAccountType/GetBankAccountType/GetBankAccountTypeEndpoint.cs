using Microsoft.EntityFrameworkCore;

public static class GetBankAccountTypeEndpoint
{
    public static void MapGetBankAccountType(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}", async (int id, RecepcionDbContext db) =>
        {
            var type = await db.BankAccountTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (type is null)
            {
                return Results.NotFound("Tipo de cuenta no encontrado.");
            }

            return Results.Ok(new ApiResponse<BankAccountTypeResponse>
            {
                Success = true,
                Message = "Tipo de cuenta encontrado.",
                StatusCode = StatusCodes.Status200OK,
                Data = BankAccountTypeEndpoints.ToResponse(type)
            });
        })
        .WithName("GetBankAccountType")
        .Produces<ApiResponse<BankAccountTypeResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
