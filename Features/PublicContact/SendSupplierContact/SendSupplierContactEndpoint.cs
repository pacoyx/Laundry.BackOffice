using System.Net;
using FluentValidation;

public static class SendSupplierContactEndpoint
{
    // Buzon fijo del negocio: si en algun momento necesita ser configurable, mover a
    // appsettings (ver patron de ChatbotSettings/WebOrdersSettings).
    private const string InboxEmail = "lavanderiakyo@gmail.com";

    public static void MapSendSupplierContact(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/public/contact/proveedores", async (
            SendSupplierContactRequest request,
            IValidator<SendSupplierContactRequest> validator,
            IEmailService emailService,
            IAppLogger<string> logger,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var categorias = request.Categorias is { Count: > 0 }
                ? string.Join(", ", request.Categorias.Select(WebUtility.HtmlEncode))
                : "No especificado";

            var message = new EmailMessage(
                new[] { InboxEmail },
                $"Nuevo proveedor interesado: {WebUtility.HtmlEncode(request.NombreComercial)}",
                $"""
                <h2>Nuevo contacto de proveedor desde la web</h2>
                <p><strong>Nombre comercial:</strong> {WebUtility.HtmlEncode(request.NombreComercial)}</p>
                <p><strong>Razón social:</strong> {WebUtility.HtmlEncode(request.RazonSocial)}</p>
                <p><strong>Contacto:</strong> {WebUtility.HtmlEncode(request.ContactoNombre)}</p>
                <p><strong>Teléfono:</strong> {WebUtility.HtmlEncode(request.Telefono)}</p>
                <p><strong>Correo:</strong> {WebUtility.HtmlEncode(request.Email)}</p>
                <p><strong>Categorías:</strong> {categorias}</p>
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
                logger.LogError("No se pudo enviar el correo de contacto de proveedor ({0}): {1}", request.Email, ex.Message);
                return Results.Problem(
                    detail: "No se pudo enviar tu información en este momento. Intenta de nuevo más tarde.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "¡Gracias! Recibimos tu información y nos pondremos en contacto contigo.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { }
            });
        })
        .WithName("SendSupplierContact")
        .WithTags("PublicContact")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("public-contact")
        .AllowAnonymous();
    }
}
