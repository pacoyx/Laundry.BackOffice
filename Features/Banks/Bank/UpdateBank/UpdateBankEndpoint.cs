using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateBankEndpoint
{
    public static void MapUpdateBank(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}", async (
            int id,
            UpdateBankRequest request,
            IValidator<UpdateBankRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var bank = await db.Banks.FirstOrDefaultAsync(x => x.Id == id);
            if (bank is null)
            {
                return Results.NotFound("Banco no encontrado.");
            }

            var nameTaken = await db.Banks.AnyAsync(x => x.Id != id && x.Name == request.Name);
            if (nameTaken)
            {
                return Results.BadRequest(new[] { $"Ya existe otro banco registrado con el nombre '{request.Name}'." });
            }

            bank.Name = request.Name;
            bank.ShortName = request.ShortName;
            bank.IsActive = request.IsActive;
            bank.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var response = new ApiResponse<BankResponse>
            {
                Success = true,
                Message = "Banco actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = BankEndpoints.ToResponse(bank)
            };

            return Results.Ok(response);
        })
        .WithName("UpdateBank")
        .Produces<ApiResponse<BankResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
