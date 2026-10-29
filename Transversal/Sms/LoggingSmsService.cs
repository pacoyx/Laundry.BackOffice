// Implementacion temporal mientras no se contrate un proveedor de SMS real (Twilio, AWS SNS, etc.).
// Solo deja el mensaje en el log de la aplicacion; no envia nada por fuera.
public class LoggingSmsService : ISmsService
{
    private readonly IAppLogger<LoggingSmsService> _logger;

    public LoggingSmsService(IAppLogger<LoggingSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phone, string message)
    {
        _logger.LogInformacion("SMS (stub, sin proveedor real) a {0}: {1}", phone, message);
        return Task.CompletedTask;
    }
}
