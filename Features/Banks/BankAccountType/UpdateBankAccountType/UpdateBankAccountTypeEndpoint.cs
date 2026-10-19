using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateBankAccountTypeEndpoint
{
    public static void MapUpdateBankAccountType(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}", async (
            int id,
            UpdateBankAccountTypeRequest request,
            IValidator<UpdateBankAccountTypeRequest> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var type = await db.BankAccountTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (type is null)
            {
                return Results.NotFound("Tipo de cuenta no encontrado.");
            }

            var nameTaken = await db.BankAccountTypes.AnyAsync(x => x.Id != id && x.Name == request.Name);
            if (nameTaken)
            {
                return Results.BadRequest(new[] { $"Ya existe otro tipo de cuenta registrado con el nombre '{request.Name}'." });
            }

            type.Name = request.Name;
            type.IsActive = request.IsActive;
            type.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var response = new ApiResponse<BankAccountTypeResponse>
            {
                Success = true,
                Message = "Tipo de cuenta actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = BankAccountTypeEndpoints.ToResponse(type)
            };

            return Results.Ok(response);
        })
        .WithName("UpdateBankAccountType")
        .Produces<ApiResponse<BankAccountTypeResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
