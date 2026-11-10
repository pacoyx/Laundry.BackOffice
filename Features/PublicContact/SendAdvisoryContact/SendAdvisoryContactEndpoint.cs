using System.Net;
using FluentValidation;

public static class SendAdvisoryContactEndpoint
{
    // Buzon fijo del negocio: si en algun momento necesita ser configurable, mover a
    // appsettings (ver patron de ChatbotSettings/WebOrdersSettings).
    private const string InboxEmail = "lavanderiakyo@gmail.com";

    public static void MapSendAdvisoryContact(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/contact/asesoria", async (
            SendAdvisoryContactRequest request,
            IValidator<SendAdvisoryContactRequest> validator,
            IEmailService emailService,
            IAppLogger<string> logger,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var message = new EmailMessage(
                new[] { InboxEmail },
                $"Nueva consulta de asesoría: {WebUtility.HtmlEncode(request.NombreCompleto)}",
                $"""
                <h2>Nueva consulta desde "Proyecto Emprendedor"</h2>
                <p><strong>Nombre completo:</strong> {WebUtility.HtmlEncode(request.NombreCompleto)}</p>
                <p><strong>Teléfono:</strong> {WebUtility.HtmlEncode(request.Telefono)}</p>
                <p><strong>Correo:</strong> {WebUtility.HtmlEncode(request.Email)}</p>
                <p><strong>Tipo de interés:</strong> {WebUtility.HtmlEncode(request.TipoInteres)}</p>
                <p><strong>Mensaje:</strong></p>
                <p>{WebUtility.HtmlEncode(request.Mensaje).Replace("\n", "<br>")}</p>
                """,
                Array.Empty<EmailAttachment>()
            );

            try
            {
                await emailService.SendAsync(message, EmailOperationCodes.CorporateOperations, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError("No se pudo enviar el correo de consulta de asesoría ({0}): {1}", request.Email, ex.Message);
                return Results.Problem(
                    detail: "No se pudo enviar tu consulta en este momento. Intenta de nuevo más tarde.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "¡Gracias por tu interés! Nos pondremos en contacto contigo para conversar sobre tu proyecto.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("SendAdvisoryContact")
        .WithTags("PublicContact")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("public-contact")
        .AllowAnonymous();
    }
}
