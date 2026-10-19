using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateBankAccountTypeEndpoint
{
    public static void MapCreateBankAccountType(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateBankAccountTypeRequest request,
            IValidator<CreateBankAccountTypeRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var nameExists = await db.BankAccountTypes.AnyAsync(x => x.Name == request.Name);
            if (nameExists)
            {
                return Results.BadRequest(new[] { $"Ya existe un tipo de cuenta registrado con el nombre '{request.Name}'." });
            }

            var type = new BankAccountType
            {
                Name = request.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await db.BankAccountTypes.AddAsync(type);
            await db.SaveChangesAsync();

            var response = new ApiResponse<BankAccountTypeResponse>
            {
                Success = true,
                Message = "Tipo de cuenta registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = BankAccountTypeEndpoints.ToResponse(type)
            };

            return Results.Created($"/api/bank-account-types/{type.Id}", response);
        })
        .WithName("CreateBankAccountType")
        .Produces<ApiResponse<BankAccountTypeResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
