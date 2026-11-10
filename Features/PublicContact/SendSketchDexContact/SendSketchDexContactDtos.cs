public record SendSketchDexContactRequest(
    string NombreCompleto,
    string NombreNegocio,
    string Telefono,
    string Email,
    string CantidadUsuarios,
    string? Mensaje);
