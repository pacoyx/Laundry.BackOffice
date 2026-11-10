using System.Net;
using FluentValidation;

public static class SendJoinUsContactEndpoint
{
    // Buzon fijo del negocio: si en algun momento necesita ser configurable, mover a
    // appsettings (ver patron de ChatbotSettings/WebOrdersSettings).
    private const string InboxEmail = "lavanderiakyo@gmail.com";

    public static void MapSendJoinUsContact(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/contact/unete", async (
            SendJoinUsContactRequest request,
            IValidator<SendJoinUsContactRequest> validator,
            IEmailService emailService,
            IAppLogger<string> logger,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var mensajeHtml = string.IsNullOrWhiteSpace(request.Mensaje)
                ? "No especificado"
                : WebUtility.HtmlEncode(request.Mensaje).Replace("\n", "<br>");

            var message = new EmailMessage(
                new[] { InboxEmail },
                $"Nueva postulación: {WebUtility.HtmlEncode(request.NombreCompleto)}",
                $"""
                <h2>Nueva postulación desde "Únete al equipo"</h2>
                <p><strong>Nombre completo:</strong> {WebUtility.HtmlEncode(request.NombreCompleto)}</p>
                <p><strong>Teléfono:</strong> {WebUtility.HtmlEncode(request.Telefono)}</p>
                <p><strong>Correo:</strong> {WebUtility.HtmlEncode(request.Email)}</p>
                <p><strong>Área de interés:</strong> {WebUtility.HtmlEncode(request.AreaInteres)}</p>
                <p><strong>Mensaje:</strong></p>
                <p>{mensajeHtml}</p>
                """,
                Array.Empty<EmailAttachment>()
            );

            try
            {
                await emailService.SendAsync(message, EmailOperationCodes.CorporateOperations, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError("No se pudo enviar el correo de postulación ({0}): {1}", request.Email, ex.Message);
                return Results.Problem(
                    detail: "No se pudo enviar tu postulación en este momento. Intenta de nuevo más tarde.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "¡Gracias por postular! Revisaremos tu información y te contactaremos.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("SendJoinUsContact")
        .WithTags("PublicContact")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("public-contact")
        .AllowAnonymous();
    }
}
