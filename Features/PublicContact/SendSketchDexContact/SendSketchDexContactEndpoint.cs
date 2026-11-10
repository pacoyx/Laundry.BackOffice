using System.Net;
using FluentValidation;

public static class SendSketchDexContactEndpoint
{
    // Buzon fijo del negocio: si en algun momento necesita ser configurable, mover a
    // appsettings (ver patron de ChatbotSettings/WebOrdersSettings).
    private const string InboxEmail = "lavanderiakyo@gmail.com";

    public static void MapSendSketchDexContact(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/contact/sketchdex", async (
            SendSketchDexContactRequest request,
            IValidator<SendSketchDexContactRequest> validator,
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
                $"Nuevo interesado en sketchDex: {WebUtility.HtmlEncode(request.NombreNegocio)}",
                $"""
                <h2>Nueva consulta desde la landing de sketchDex</h2>
                <p><strong>Nombre completo:</strong> {WebUtility.HtmlEncode(request.NombreCompleto)}</p>
                <p><strong>Negocio:</strong> {WebUtility.HtmlEncode(request.NombreNegocio)}</p>
                <p><strong>Teléfono:</strong> {WebUtility.HtmlEncode(request.Telefono)}</p>
                <p><strong>Correo:</strong> {WebUtility.HtmlEncode(request.Email)}</p>
                <p><strong>Cantidad de usuarios aproximada:</strong> {WebUtility.HtmlEncode(request.CantidadUsuarios)}</p>
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
                logger.LogError("No se pudo enviar el correo de contacto sketchDex ({0}): {1}", request.Email, ex.Message);
                return Results.Problem(
                    detail: "No se pudo enviar tu consulta en este momento. Intenta de nuevo más tarde.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "¡Gracias por tu interés en sketchDex! Nos pondremos en contacto contigo para coordinar una demo.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("SendSketchDexContact")
        .WithTags("PublicContact")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("public-contact")
        .AllowAnonymous();
    }
}
