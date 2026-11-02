public class BrevoSmsSettings
{
    public string ApiKey { get; set; } = string.Empty;

    // Nombre/remitente que aparece en el SMS (alfanumerico, maximo 11 caracteres segun Brevo).
    public string Sender { get; set; } = string.Empty;
}
