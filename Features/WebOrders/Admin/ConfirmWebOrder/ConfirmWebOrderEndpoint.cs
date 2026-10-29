using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class ConfirmWebOrderEndpoint
{
    private const string TypeProcess = "WEB";

    public static void MapConfirmWebOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{id:long}/confirm", async (long id, ClaimsPrincipal principal, RecepcionDbContext db, IAppLogger<string> logger) =>
        {
            var webOrder = await db.WebOrders
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (webOrder is null)
            {
                return Results.NotFound();
            }

            if (webOrder.Status != WebOrderStatus.Pendiente)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Solo se pueden confirmar pedidos pendientes.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var centralBranch = await db.BranchSales.FirstOrDefaultAsync(x => x.IsCentral);
            if (centralBranch is null)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No hay una sucursal marcada como central. Configúrela en Maestros.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var numbersDocument = await db.NumbersDocuments
                .FirstOrDefaultAsync(nd => nd.BranchId == centralBranch.Id && nd.TypeProcess == TypeProcess);

            if (numbersDocument is null)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No se encontró el correlativo de pedidos web (WEB) para la sucursal central. Configúrelo en Maestros.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            // Ubicacion no es un codigo de letra: guarda el Name real de un LocationClothes,
            // igual que ya hace AssignWorkGuideDetailLocationEndpoint. Un pedido web recien
            // confirmado arranca en Lavanderia, no en Almacen.
            var lavanderia = await db.LocationClothes.FirstOrDefaultAsync(x => x.Name == "Lavandería");
            var ubicacionInicial = lavanderia?.Name ?? "";

            var workGuideMain = new WorkGuideMain
            {
                SerieGuia = numbersDocument.SerieDoc,
                NumeroGuia = numbersDocument.NumberDoc.ToString(),
                FechaOperacion = now,
                FechaHoraEntrega = now.AddDays(2),
                MensajeAlertas = "",
                Observaciones = "Generada desde pedido web " + webOrder.OrderNumber,
                TipoPago = "SP",
                DescripcionPago = "",
                TipoRecepcion = "D",
                DireccionContacto = webOrder.DeliveryAddress,
                TelefonoContacto = webOrder.ContactPhone,
                Total = webOrder.Total,
                Acuenta = 0,
                Saldo = webOrder.Total,
                CustomerId = webOrder.CustomerId,
                BranchId = centralBranch.Id,
                EstadoPago = "PE",
                EstadoRegistro = "A",
                EstadoSituacion = "P",
                WorkGuideDetails = webOrder.Details.Select(d => new WorkGuideDetail
                {
                    Cant = d.Quantity,
                    Precio = d.UnitPrice,
                    Total = d.Total,
                    Observaciones = "",
                    TipoLavado = "",
                    Ubicacion = ubicacionInicial,
                    EstadoTrabajo = "P",
                    ProductId = d.ServiceItemId,
                    EstadoRegistro = "A",
                    EstadoSituacion = "P",
                    EstadoPago = "PE",
                    Identificador = ""
                }).ToList()
            };

            db.WorkGuideMains.Add(workGuideMain);

            numbersDocument.NumberDoc += 1;
            db.NumbersDocuments.Update(numbersDocument);

            await db.SaveChangesAsync();

            webOrder.WorkGuideMainId = workGuideMain.Id;
            webOrder.Status = WebOrderStatus.Confirmado;
            webOrder.ConfirmedAt = now;
            webOrder.ConfirmedByUserId = principal.GetUserId();

            await db.SaveChangesAsync();

            logger.LogInformacion("Pedido web {0} confirmado, genera guia {1}-{2}", webOrder.OrderNumber, workGuideMain.SerieGuia, workGuideMain.NumeroGuia);

            return Results.Ok(new ApiResponse<ConfirmWebOrderResponse>
            {
                Success = true,
                Message = "Pedido confirmado y guía generada.",
                StatusCode = StatusCodes.Status200OK,
                Data = new ConfirmWebOrderResponse(webOrder.Id, workGuideMain.Id, workGuideMain.SerieGuia, workGuideMain.NumeroGuia)
            });
        })
        .WithName("ConfirmWebOrder")
        .WithTags("WebOrders")
        .Produces<ApiResponse<ConfirmWebOrderResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequirePermission(PermissionCodes.ReceptionPedidosWeb);
    }
}
