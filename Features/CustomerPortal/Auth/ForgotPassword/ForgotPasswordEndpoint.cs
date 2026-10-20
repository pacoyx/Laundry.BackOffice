using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public static class ForgotPasswordEndpoint
{
    public static void MapForgotPassword(this IEndpointRouteBuilder app)
    {
        app.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            RecepcionDbContext db,
            IOptions<CustomerPortalSettings> portalSettings,
            IEmailService emailService,
            IAppLogger<string> logger) =>
        {
            var user = await db.CustomerUsers.FirstOrDefaultAsync(x => x.UserName == request.UserName);

            // El token NUNCA se devuelve en la respuesta: solo se envía al correo registrado del
            // usuario. La respuesta es idéntica exista o no el usuario, para no permitir enumeración
            // de usernames ni habilitar un account takeover vía Postman/DevTools.
            if (user is not null && user.IsActive && !string.IsNullOrWhiteSpace(user.Email))
            {
                var pendingTokens = await db.CustomerUserTokens
                    .Where(x => x.CustomerUserId == user.Id && x.Purpose == CustomerUserTokenPurpose.PasswordReset && !x.IsUsed)
                    .ToListAsync();
                foreach (var pending in pendingTokens)
                {
                    pending.IsUsed = true;
                }

                var token = new CustomerUserToken
                {
                    CustomerUserId = user.Id,
                    Token = CustomerPortalTokenGenerator.Generate(),
                    Purpose = CustomerUserTokenPurpose.PasswordReset,
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    IsUsed = false
                };

                await db.CustomerUserTokens.AddAsync(token);
                await db.SaveChangesAsync();

                var resetUrl = $"{portalSettings.Value.BaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token.Token)}";
                var safeFullName = WebUtility.HtmlEncode(user.FullName);

                var message = new EmailMessage(
                    new[] { user.Email },
                    "Recuperación de contraseña - Portal de Clientes",
                    $"""
                    <p>Hola {safeFullName},</p>
                    <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta del portal de clientes.</p>
                    <p><a href="{resetUrl}">Haz clic aquí para crear una nueva contraseña</a></p>
                    <p>Este enlace expira en 2 horas. Si no solicitaste este cambio, puedes ignorar este correo.</p>
                    """,
                    Array.Empty<EmailAttachment>()
                );

                try
                {
                    await emailService.SendAsync(message, EmailOperationCodes.CorporateOperations);
                }
                catch (Exception ex)
                {
                    // No se expone el error al caller: revelarlo permitiría inferir que el usuario existe.
                    logger.LogError("No se pudo enviar el correo de recuperación de contraseña para el usuario {0}: {1}", user.UserName, ex.Message);
                }
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Si el usuario existe, se envió un correo con instrucciones para restablecer la contraseña.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("ForgotCustomerPassword")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}
