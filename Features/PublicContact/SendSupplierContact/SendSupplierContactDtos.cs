public record SendSupplierContactRequest(
    string NombreComercial,
    string RazonSocial,
    string ContactoNombre,
    string Telefono,
    string Email,
    List<string> Categorias,
    string Mensaje);
