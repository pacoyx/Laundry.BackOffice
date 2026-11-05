using Microsoft.EntityFrameworkCore;

public static class GetCustomersEndpoint
{
    public static void MapGetCustomers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{pageNumber}/{pageSize}", async (int pageNumber, int pageSize, CustomerType? customerType, RecepcionDbContext context) =>
        {
            // Aplicar un filtro (por ejemplo, solo clientes activos)
            var query = context.Customers.AsNoTracking();

            if (customerType.HasValue)
            {
                query = query.Where(c => c.CustomerType == customerType.Value);
            }

             // Obtener el total de filas después de aplicar el filtro
            var totalRows = await query.CountAsync();

            // Obtener los clientes paginados después de aplicar el filtro
            var pagedCustomers = await query
                .OrderBy(c => c.FirtsName)
                .ThenBy(c => c.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responsePaginator = new GetCustomersResponseDto(
                totalRows,
                pagedCustomers.Select(c => new GetCustomersResponseDatosDto(
                    c.Id,
                    c.FirtsName,
                    c.LastName,
                    c.Address,
                    c.Phone,
                    c.Email,
                    c.DocPersonal,
                    c.Status,
                    c.CustomerType,
                    c.InternalCode
                )).ToList()
            );

            var response = new ApiResponse<GetCustomersResponseDto>
            {
                Data = responsePaginator,
                Success = true,
                Message = "",
                StatusCode = 200
            };

           
            return Results.Ok(response);
        })
         // aplicar a la ruta el atributo de autorización
        .RequireAuthorization();

    }
}