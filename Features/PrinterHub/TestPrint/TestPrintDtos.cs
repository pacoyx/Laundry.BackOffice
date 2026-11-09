namespace _24dex_backend_comercial.Features.PrinterHub;

// Payload que recibe el worker por SignalR (ver LaundryPrintWorker, handler "PrintTestTicket").
public record TestPrintCommandDto(string PrinterName, DateTime Timestamp);
