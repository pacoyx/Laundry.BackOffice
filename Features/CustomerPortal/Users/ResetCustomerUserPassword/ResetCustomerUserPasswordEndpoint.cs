using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

public static class ResetCustomerUserPasswordEndpoint
{
    private const string UpperChars = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // sin I, O
    private const string LowerChars = "abcdefghijkmnopqrstuvwxyz"; // sin l
    private const string DigitChars = "23456789"; // sin 0, 1

    public static void MapResetCustomerUserPassword(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{id:long}/reset-password", async (
            long id,
            ClaimsPrincipal principal,
            RecepcionDbContext db,
            IEncryptService encryptService,
            IEmailService emailService) =>
        {
            var customerId = principal.GetCustomerId();
            if (customerId is null)
            {
                return Results.Unauthorized();
            }

            // Filtrado por CustomerId del token: no permite resetear usuarios de otra empresa.
            var user = await db.CustomerUsers
                .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId.Value);

            if (user is null)
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return Results.BadRequest(new[] { "El usuario no tiene un correo registrado; no se puede enviar la nueva contraseña." });
            }

            var newPassword = GenerateRandomPassword();
            var safeFullName = WebUtility.HtmlEncode(user.FullName);

            var message = new EmailMessage(
                new[] { user.Email },
                "Restablecimiento de contraseña - Portal de Clientes",
                $"""
                <p>Hola {safeFullName},</p>
                <p>Se generó una nueva contraseña para tu cuenta del portal de clientes:</p>
                <p style="font-size:18px;font-weight:bold;letter-spacing:1px;">{newPassword}</p>
                <p>Por seguridad, te recomendamos iniciar sesión y cambiarla en cuanto puedas.</p>
                <p>Si no solicitaste este cambio, comunícate con el administrador de tu empresa.</p>
                """,
                Array.Empty<EmailAttachment>()
            );

            // El correo se envía ANTES de guardar el nuevo hash: si el envío falla, el usuario
            // no se queda bloqueado con una contraseña que nunca llegó a ver.
            try
            {
                await emailService.SendAsync(message, EmailOperationCodes.CorporateOperations);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    $"No se pudo enviar el correo con la nueva contraseña: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            // Invalida cualquier link de recuperación pendiente para que no quede un token huérfano.
            var pendingTokens = await db.CustomerUserTokens
                .Where(x => x.CustomerUserId == id && x.Purpose == CustomerUserTokenPurpose.PasswordReset && !x.IsUsed)
                .ToListAsync();
            foreach (var pending in pendingTokens)
            {
                pending.IsUsed = true;
            }

            user.PasswordHash = encryptService.HashPassword(newPassword);
            user.RefreshToken = null;
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Se generó una nueva contraseña y se envió al correo registrado del usuario ({user.Email}). Solo el usuario podrá verla revisando su bandeja de entrada.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { user.Id, user.Email }
            });
        })
        .WithName("ResetCustomerPortalUserPassword")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .RequireCustomerPermission(CustomerPermissionCodes.UsersManage);
    }

    private static string GenerateRandomPassword(int length = 12)
    {
        const string allChars = UpperChars + LowerChars + DigitChars;

        var chars = new char[length];
        chars[0] = UpperChars[RandomNumberGenerator.GetInt32(UpperChars.Length)];
        chars[1] = LowerChars[RandomNumberGenerator.GetInt32(LowerChars.Length)];
        chars[2] = DigitChars[RandomNumberGenerator.GetInt32(DigitChars.Length)];

        for (var i = 3; i < length; i++)
        {
            chars[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
        }

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
