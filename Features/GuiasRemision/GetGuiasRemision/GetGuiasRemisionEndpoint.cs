using Microsoft.EntityFrameworkCore;

public static class GetGuiasRemisionEndpoint
{
    public static void MapGetGuiasRemision(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            string? invoiceNumero,
            DateTime? desde,
            DateTime? hasta,
            RecepcionDbContext db) =>
        {
            var query = db.GuiasRemision
                .AsNoTracking()
                .Include(x => x.Invoice).ThenInclude(i => i.Customer).ThenInclude(c => c.BillingProfile)
                .Include(x => x.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(invoiceNumero))
            {
                query = query.Where(x => x.Invoice.Numero.Contains(invoiceNumero));
            }

            if (desde is not null)
            {
                query = query.Where(x => x.FechaTraslado >= desde.Value.Date);
            }

            if (hasta is not null)
            {
                query = query.Where(x => x.FechaTraslado <= hasta.Value.Date);
            }

            var guias = await query
                .OrderByDescending(x => x.FechaEmision)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<GuiaRemisionResponse>>
            {
                Success = true,
                Message = "Listado de guías de remisión.",
                StatusCode = StatusCodes.Status200OK,
                Data = guias.Select(GuiasRemisionEndpoints.ToResponse).ToList()
            });
        })
        .WithName("GetGuiasRemision")
        .Produces<ApiResponse<List<GuiaRemisionResponse>>>(StatusCodes.Status200OK);
    }
}
