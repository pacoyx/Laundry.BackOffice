
public record ResponseSearchCustomerDtoPaginator(
  int TotalCount,
  List<ResponseSearchCustomerDtoDetailpaginator> Customers
  );


public record ResponseSearchCustomerDtoDetailpaginator(
int Id,
string FirstName,
string? LastName,
string? Address,
string? Phone,
string? Email,
string? DocPersonal,
string? Status,
CustomerType CustomerType,
string? InternalCode
);

public record ResponseSearchCustomerDto(
  int id,
  string codigo,
  string nombres,
  string apellidos,
  string telefono
);

public record ResponseSearchCustomerByPatronDto(
int Id,
string Name,
string Ruc
);
