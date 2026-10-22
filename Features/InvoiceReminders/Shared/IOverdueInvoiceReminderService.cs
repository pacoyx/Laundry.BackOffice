public interface IOverdueInvoiceReminderService
{
    // Agrupa facturas vencidas por cliente, resuelve destinatarios/supresiones, envia un
    // correo por cliente y registra un InvoiceReminderLog por cada uno. Nunca lanza por un
    // fallo de un cliente puntual (queda como Failed en su log); si lanza, es por un error
    // de configuracion (ej. la config no existe).
    Task ExecuteAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default);

    // Resuelve asunto/cuerpo para UN cliente puntual, sin enviar ni registrar. Lanza si el
    // cliente no tiene facturas vencidas actualmente (no hay nada que previsualizar).
    Task<(string Subject, string Body)> PreviewAsync(int scheduledEmailNotificationId, int customerId, CancellationToken cancellationToken = default);

    // Envia a una direccion puntual con los datos reales de ese cliente, sin registrar
    // ejecucion ni tocar Hangfire. Propaga excepciones.
    Task SendTestAsync(int scheduledEmailNotificationId, int customerId, string testEmail, CancellationToken cancellationToken = default);
}
