using Microsoft.EntityFrameworkCore;

public static class PickUpItemEndpoint
{
    public static void MapPickUpItem(this IEndpointRouteBuilder app)
    {
        app.MapPut("/pickUpItem/{id}", async (RecepcionDbContext db, int id, RequestRecogerItemDto request, IAppLogger<string> logger) =>
        {
            var workGuideItem = await db.WorkGuideDetails.FindAsync(id);
            if (workGuideItem == null)
            {
                logger.LogWarning("Item no encontrado" + id.ToString(), "PickUpItemEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "Item no encontrado",
                    StatusCode = 404,
                    Success = false
                };
                return Results.NotFound(responseValidation);
            }

            if (workGuideItem.EstadoSituacion == "E")
            {
                logger.LogWarning("El item ya se entrego" + id.ToString(), "PickUpItemEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "El item ya se entrego",
                    StatusCode = 400,
                    Success = false
                };
                return Results.BadRequest(responseValidation);
            }

            if (workGuideItem.EstadoSituacion == "D")
            {
                logger.LogWarning("El item ya se encuentra devuelto" + id.ToString(), "PickUpItemEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "El item ya se encuentra devuelto",
                    StatusCode = 400,
                    Success = false
                };
                return Results.BadRequest(responseValidation);
            }

            if (workGuideItem.EstadoRegistro == "I")
            {
                logger.LogWarning("El item esta anulado" + id.ToString(), "PickUpItemEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "El item esta anulado",
                    StatusCode = 400,
                    Success = false
                };
                return Results.BadRequest(responseValidation);
            }

            if (request.CobrarEfectivo)
            {
                if (request.Monto <= 0)
                {
                    logger.LogWarning("El monto debe ser mayor a 0" + id.ToString(), "PickUpItemEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "El monto debe ser mayor a 0",
                        StatusCode = 400,
                        Success = false
                    };
                    return Results.BadRequest(responseValidation);
                }

                if (request.Monto > workGuideItem.Total)
                {
                    logger.LogWarning("El monto no puede ser mayor al total" + id.ToString(), "PickUpItemEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "El monto no puede ser mayor al total",
                        StatusCode = 400,
                        Success = false
                    };
                    return Results.BadRequest(responseValidation);
                }

                var workGuide = await db.WorkGuideMains.FindAsync(workGuideItem.WorkGuideMainId);
                if (workGuide == null)
                {
                    logger.LogWarning("No se encontro guia de trabajo" + id.ToString(), "PickUpItemEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "No se encontro guia de trabajo",
                        StatusCode = 404,
                        Success = false
                    };
                    return Results.NotFound(responseValidation);
                }

                workGuide.Saldo = workGuide.Saldo - request.Monto;

                var cashBoxMain = await db.CashBoxMains.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.EstadoRegistro == "A" && x.EstadoCaja == "A");
                if (cashBoxMain == null)
                {
                    logger.LogWarning("No se encontro caja principal" + id.ToString(), "PickUpItemEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "No se encontro caja principal",
                        StatusCode = 404,
                        Success = false
                    };
                    return Results.NotFound(responseValidation);
                }

                var cashBoxDetail = new CashBoxDetail
                {
                    CashBoxMainId = cashBoxMain.Id,
                    EstadoRegistro = "A",
                    TipoComprobante = "GS",
                    SerieComprobante = workGuide.SerieGuia,
                    NumComprobante = workGuide.NumeroGuia,
                    FechaComprobante = workGuide.FechaOperacion,
                    Importe = request.Monto,
                    Adelanto = 0,
                    TipoPago = request.TipoPago,
                    DescripcionPago = request.DescripcionPago,
                    Observaciones = "Cobro de item de guia de trabajo",
                    CustomerId = workGuide.CustomerId
                };

                db.CashBoxDetails.Add(cashBoxDetail);

                if(request.Monto == workGuideItem.Total)
                {
                    workGuideItem.EstadoPago = "PA";
                }                   
            }

            DateTime fechaProceso = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            workGuideItem.EstadoSituacion = "E";
            workGuideItem.FechaRecojo = fechaProceso;
            await db.SaveChangesAsync();

            var response = new ApiResponse<string>()
            {
                Data = "El item fue entregado con exito",
                Message = "El item fue entregado con exito",
                Success = true,
                StatusCode = 200
            };

            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionRetiro);
    }
}