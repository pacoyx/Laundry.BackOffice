public interface IScheduledEmailService
{
    // Resuelve la configuracion, arma el correo con los destinatarios TO/BCC configurados,
    // lo envia y registra el resultado en el historial de ejecuciones. Nunca lanza: una falla
    // de envio queda reflejada como ScheduledEmailExecutionStatus.Failed, no como excepcion.
    Task<ScheduledEmailExecution> ExecuteAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default);

    // Envia un correo de prueba a una direccion puntual (no a los destinatarios configurados),
    // sin registrar ejecucion ni tocar la programacion de Hangfire. Propaga excepciones.
    Task SendTestAsync(int scheduledEmailNotificationId, string testEmail, CancellationToken cancellationToken = default);

    // Resuelve asunto y cuerpo finales sin enviar nada ni registrar ejecucion.
    Task<(string Subject, string Body)> PreviewAsync(int scheduledEmailNotificationId, CancellationToken cancellationToken = default);
}
