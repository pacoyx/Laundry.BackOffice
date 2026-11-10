public record SendJoinUsContactRequest(
    string NombreCompleto,
    string Telefono,
    string Email,
    string AreaInteres,
    string? Mensaje);
