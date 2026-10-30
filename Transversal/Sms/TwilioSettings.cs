public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;

    // Numero de Twilio que envia el SMS (formato E.164, ej. "+15017122661").
    public string FromPhoneNumber { get; set; } = string.Empty;
}
