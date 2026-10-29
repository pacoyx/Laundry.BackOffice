public class WebOrdersSettings
{
    public int OtpExpirationMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 5;
    public int OtpResendCooldownSeconds { get; set; } = 60;
    public int OtpMaxRequestsPerHour { get; set; } = 5;
}
