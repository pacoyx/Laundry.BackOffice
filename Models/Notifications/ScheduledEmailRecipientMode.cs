public enum ScheduledEmailRecipientMode
{
    // Lista fija guardada en ScheduledEmailRecipients (caso chofer de recojo).
    Fixed = 0,

    // Destinatarios resueltos dinamicamente por el propio job (caso facturas
    // vencidas: uno por cliente, segun CustomerContact.ReceiveInvoices).
    Dynamic = 1
}
