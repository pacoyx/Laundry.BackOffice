using Microsoft.EntityFrameworkCore;

public static class GetWorkGuideByDocumentEndpoint
{
    public static void MapGetWorkGuideByDocument(this IEndpointRouteBuilder app)
    {
        app.MapGet("/document/{serie}/{numero}", async (string serie, string numero, RecepcionDbContext db) =>
        {
            if (string.IsNullOrEmpty(serie) || string.IsNullOrEmpty(numero))
            {
                return Results.BadRequest("Serie and Number must not be empty");
            }

            var workGuideMain = await db.WorkGuideMains
            .Include(w => w.Customer)
            .Include(w => w.WorkGuideDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(w => w.SerieGuia == serie && w.NumeroGuia == numero);

            if (workGuideMain == null)
            {
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "WorkGuideMain not found",
                    StatusCode = 404,
                    Success = false
                };
                return Results.NotFound(responseValidation);
            }

            var detailIds = workGuideMain.WorkGuideDetails.Select(d => d.Id).ToList();
            var latestLocations = await db.LocationWorkGuides
                .Where(x => x.WorkGuideDetailId != null && detailIds.Contains(x.WorkGuideDetailId.Value))
                .GroupBy(x => x.WorkGuideDetailId!.Value)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToDictionaryAsync(x => x.WorkGuideDetailId!.Value);

            var responseWgmDto = new ResponseGuiaByDocumentDto(
                workGuideMain.Id,
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
                workGuideMain.Customer!.FirtsName + ' ' + workGuideMain.Customer.LastName,
                workGuideMain.Customer.Phone,
                workGuideMain.EstadoPago,
                workGuideMain.FechaPago,
                workGuideMain.EstadoRegistro,
                workGuideMain.EstadoSituacion,
                workGuideMain.FechaRecojo,
                workGuideMain.TipoPagoCancelacion,
                workGuideMain.WorkGuideDetails.Select(d => new ResponseGuiaByDocumentDtoDet(
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
                    d.FechaRecojo,
                    d.FechaDevolucion,
                    d.Identificador,
                    latestLocations.TryGetValue(d.Id, out var loc1) ? loc1.CreatedDate : null,
                    latestLocations.TryGetValue(d.Id, out var loc2) ? loc2.FechaEntregaAprox : null
                ))
            );

            var response = new ApiResponse<ResponseGuiaByDocumentDto>()
            {
                Data = responseWgmDto,
                Message = "guias por encontradas por documento",
                StatusCode = 200,
                Success = true
            };
            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionConsultas);
    }
}
