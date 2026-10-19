using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateBankEndpoint
{
    public static void MapCreateBank(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateBankRequest request,
            IValidator<CreateBankRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var nameExists = await db.Banks.AnyAsync(x => x.Name == request.Name);
            if (nameExists)
            {
                return Results.BadRequest(new[] { $"Ya existe un banco registrado con el nombre '{request.Name}'." });
            }

            var bank = new Bank
            {
                Name = request.Name,
                ShortName = request.ShortName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await db.Banks.AddAsync(bank);
            await db.SaveChangesAsync();

            var response = new ApiResponse<BankResponse>
            {
                Success = true,
                Message = "Banco registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = BankEndpoints.ToResponse(bank)
            };

            return Results.Created($"/api/banks/{bank.Id}", response);
        })
        .WithName("CreateBank")
        .Produces<ApiResponse<BankResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
