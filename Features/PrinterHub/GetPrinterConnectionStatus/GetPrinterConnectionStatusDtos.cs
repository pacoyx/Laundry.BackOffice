namespace _24dex_backend_comercial.Features.PrinterHub;

public record PrinterConnectionStatusResponse(
    bool IsConnected,
    int? PrinterId,
    string? PrinterName,
    DateTime? LastConnectionAt,
    string? Message);
