using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class UpdateEmailAccountEndpoint
{
    public static void MapUpdateEmailAccount(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}", async (
            int id,
            UpdateEmailAccountRequest request,
            IValidator<UpdateEmailAccountRequest> validator,
            RecepcionDbContext db,
            ISymmetricEncryptionService encryptionService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var account = await db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == id);
            if (account is null)
            {
                return Results.NotFound("Cuenta de correo no encontrada.");
            }

            var operacionTaken = await db.EmailAccounts.AnyAsync(x => x.Id != id && x.Operacion == request.Operacion);
            if (operacionTaken)
            {
                return Results.BadRequest(new[] { $"Ya existe otra cuenta de correo registrada para la operación '{request.Operacion}'." });
            }

            account.Operacion = request.Operacion;
            account.Provider = request.Provider;
            account.Host = request.Host;
            account.Port = request.Port;
            account.UserName = request.UserName;
            account.EnableSsl = request.EnableSsl;
            account.IsActive = request.IsActive;
            account.UpdatedAt = DateTime.UtcNow;

            // Password opcional: si no se envía, se conserva la contraseña vigente.
            if (!string.IsNullOrEmpty(request.Password))
            {
                account.Password = encryptionService.Encrypt(request.Password);
            }

            await db.SaveChangesAsync();

            var response = new ApiResponse<EmailAccountResponse>
            {
                Success = true,
                Message = "Cuenta de correo actualizada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = EmailAccountsEndpoints.ToResponse(account)
            };

            return Results.Ok(response);
        })
        .WithName("UpdateEmailAccount")
        .Produces<ApiResponse<EmailAccountResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
