using Microsoft.EntityFrameworkCore;

public static class GetCustomerGarmentHistoryEndpoint
{
    public static void MapGetCustomerGarmentHistory(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{customerId}", async (int customerId, string? search, RecepcionDbContext db) =>
        {
            var query = db.CustomerGarmentHistories
                .AsNoTracking()
                .Where(h => h.CustomerId == customerId && h.Status == "A");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h => h.Product!.Name.Contains(search) || h.Detail.Contains(search));
            }

            var items = await query
                .OrderBy(h => h.Product!.Name)
                .ThenBy(h => h.Detail)
                .Select(h => new CustomerGarmentHistoryResponseDto(
                    h.Id,
                    h.ProductId,
                    h.Product!.Name,
                    h.Product!.Price,
                    h.Detail,
                    h.UsageCount,
                    h.LastUsedAt
                ))
                .ToListAsync();

            var response = new ApiResponse<List<CustomerGarmentHistoryResponseDto>>
            {
                Data = items,
                Success = true,
                Message = "",
                StatusCode = StatusCodes.Status200OK
            };

            return Results.Ok(response);
        });
    }
}
