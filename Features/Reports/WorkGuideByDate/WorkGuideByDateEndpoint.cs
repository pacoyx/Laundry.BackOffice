using Microsoft.EntityFrameworkCore;
using System.Globalization;

public static class WorkGuideByDateEndpoint
{
    public static void MapWorkGuideByDate(this IEndpointRouteBuilder app)
    {
        app.MapGet("/workguides/bydate/{pageNumber}/{pageSize}", async (DateTime date, int pageNumber, int pageSize, RecepcionDbContext dbContext) =>
        {
            
            var query = dbContext.WorkGuideMains
            .AsNoTracking()
            .Include(wg => wg.Customer)
            .Include(wg => wg.WorkGuideDetails)
            .ThenInclude(wgd => wgd.Product)
            .Where(wg => wg.FechaOperacion.Date == date.Date);

            var totalRows = await query.CountAsync();

            var workGuides = await query
            .OrderByDescending(c => c.FechaOperacion)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)            
            .ToListAsync();

            var responsePaginatorDto = new GetWgByDatePaginatorResponseDto(
                totalRows,
                workGuides.Select(wg => new WgByDateResponseDto(
                    wg.Id,
                    wg.SerieGuia,
                    wg.NumeroGuia,
                    wg.FechaOperacion,
                    wg.FechaHoraEntrega,
                    wg.Observaciones,
                    wg.TipoPago,
                    wg.DescripcionPago,
                    wg.TipoRecepcion,
                    wg.Total,
                    wg.Acuenta,
                    wg.Saldo,
                    wg.Customer != null ? wg.Customer.FirtsName + " " + wg.Customer.LastName : string.Empty,
                    wg.Customer?.Phone ?? string.Empty,
                    wg.EstadoPago,
                    wg.FechaPago,
                    wg.EstadoRegistro,
                    wg.EstadoSituacion,
                    wg.FechaRecojo,
                    wg.TipoPagoCancelacion,
                    wg.WorkGuideDetails.Select(wgd => new WgByDateDetailResponseDto
                    {
                        Id = wgd.Id,
                        Cant = wgd.Cant,
                        Precio = wgd.Precio,
                        Total = wgd.Total,
                        Servicio = wgd.Product?.Name ?? string.Empty,
                        Observaciones = wgd.Observaciones,
                        Ubicacion = wgd.Ubicacion,
                        EstadoTrabajo = wgd.EstadoTrabajo,
                        EstadoRegistro = wgd.EstadoRegistro,
                        EstadoSituacion = wgd.EstadoSituacion,
                        EstadoPago = wgd.EstadoPago,
                        FechaRecojo = wgd.FechaRecojo,
                        FechaDevolucion = wgd.FechaDevolucion,
                        Identificador = wgd.Identificador
                    }).ToArray()
                )).ToList()
            );
            
            var response = new ApiResponse<GetWgByDatePaginatorResponseDto>
            {
                Data = responsePaginatorDto,
                Success = true,
                Message = "",
                StatusCode = 200
            };

            return Results.Ok(response);
        })
        .WithName("GetWorkGuidesByDate");
    }



    
}