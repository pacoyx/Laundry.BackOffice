using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateEmailAccountEndpoint
{
    public static void MapCreateEmailAccount(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateEmailAccountRequest request,
            IValidator<CreateEmailAccountRequest> validator,
            RecepcionDbContext db,
            ISymmetricEncryptionService encryptionService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var operacionExists = await db.EmailAccounts.AnyAsync(x => x.Operacion == request.Operacion);
            if (operacionExists)
            {
                return Results.BadRequest(new[] { $"Ya existe una cuenta de correo registrada para la operación '{request.Operacion}'." });
            }

            var account = new EmailAccount
            {
                Operacion = request.Operacion,
                Provider = request.Provider,
                Host = request.Host,
                Port = request.Port,
                UserName = request.UserName,
                Password = encryptionService.Encrypt(request.Password),
                EnableSsl = request.EnableSsl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await db.EmailAccounts.AddAsync(account);
            await db.SaveChangesAsync();

            var response = new ApiResponse<EmailAccountResponse>
            {
                Success = true,
                Message = "Cuenta de correo registrada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = EmailAccountsEndpoints.ToResponse(account)
            };

            return Results.Created($"/api/email-accounts/{account.Id}", response);
        })
        .WithName("CreateEmailAccount")
        .Produces<ApiResponse<EmailAccountResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
