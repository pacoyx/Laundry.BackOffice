using Microsoft.EntityFrameworkCore;

public static class GetWorkGuideMainEndpoint
{
    public static void MapGetWorkGuideMain(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (int id, RecepcionDbContext db) =>
        {

            if (id <= 0){
                return Results.BadRequest("Id must be greater than zero");                
            }

            var workGuideMain = await db.WorkGuideMains
            .Include(w => w.Customer)
            .Include(w => w.WorkGuideDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(w => w.Id == id);

            if (workGuideMain == null)
            {
                return Results.NotFound();
            }

            var detailIds = workGuideMain.WorkGuideDetails.Select(d => d.Id).ToList();
            var latestLocations = await db.LocationWorkGuides
                .Where(x => x.WorkGuideDetailId != null && detailIds.Contains(x.WorkGuideDetailId.Value))
                .GroupBy(x => x.WorkGuideDetailId!.Value)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToDictionaryAsync(x => x.WorkGuideDetailId!.Value);

            var responseWgmDto = new ResponseWgmDto(
                workGuideMain.SerieGuia,
                workGuideMain.NumeroGuia,
                workGuideMain.FechaOperacion,
                workGuideMain.FechaHoraEntrega,
                workGuideMain.MensajeAlertas,
                workGuideMain.Observaciones,
                workGuideMain.TipoPago,
                workGuideMain.DescripcionPago,
                workGuideMain.TipoRecepcion,
                workGuideMain.DireccionContacto,
                workGuideMain.TelefonoContacto,
                workGuideMain.Total,
                workGuideMain.Acuenta,
                workGuideMain.Saldo,
                workGuideMain.CustomerId,
                workGuideMain.Customer!.FirtsName + ' '+ workGuideMain.Customer.LastName,
                workGuideMain.Customer.Phone,
                workGuideMain.EstadoPago,
                workGuideMain.FechaPago,
                workGuideMain.EstadoRegistro,
                workGuideMain.EstadoSituacion,
                workGuideMain.FechaRecojo,
                workGuideMain.WorkGuideDetails.Select(d => new ResponseWgdDto(
                    d.Id,
                    d.Cant,
                    d.Precio,
                    d.Total,
                    d.Observaciones,
                    d.TipoLavado,
                    d.Ubicacion,
                    d.EstadoTrabajo,
                    d.ProductId,
                    d.Product!,
                    d.EstadoRegistro,
                    d.EstadoSituacion,
                    d.EstadoPago,
                    latestLocations.TryGetValue(d.Id, out var loc1) ? loc1.CreatedDate : null,
                    latestLocations.TryGetValue(d.Id, out var loc2) ? loc2.FechaEntregaAprox : null
                ))
            );           
            var response = new ApiResponse<ResponseWgmDto>(){
                Data = responseWgmDto,
                Message = "WorkGuideMain found",
                StatusCode = 200,
                Success = true
            };
            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionConsultas);
    }
}