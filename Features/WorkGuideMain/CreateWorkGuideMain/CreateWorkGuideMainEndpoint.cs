
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using _24dex_backend_comercial.Features.WorkGuideMain.CreateWorkGuideMain;
using DexterCompany.Models;
using Microsoft.EntityFrameworkCore;


public static class CreateWorkGuideMainEndpoint
{
    public static void MapCreateWorkGuideMain(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (WgmCreateDto wgmCreateDto, RecepcionDbContext db, IAppLogger<string> logger,IPrintNotifier printNotifier) =>
        {

            if (wgmCreateDto == null)
            {
                logger.LogWarning("Request vacio", "CreateWorkGuideMainEndpoint");
                return Results.BadRequest("Request vacio");
            }

            var TYPE_PROCESS = "RECEP";

            //obtener el ultimo correlativo de la guia de servicio
            var numbersDocument = await db.NumbersDocuments
                .FirstOrDefaultAsync(nd => nd.BranchId == wgmCreateDto.BranchStoreId
                                        && nd.TypeProcess == TYPE_PROCESS);
            if (numbersDocument == null)
            {
                logger.LogWarning("No se encontró correlativo de guia de servicio", "CreateWorkGuideMainEndpoint");
                return Results.BadRequest("No se encontró correlativo de guia de servicio");
            }

            //validar si el correlativo de la guia de servicio es igual al enviado
            // if (numbersDocument.NumberDoc != int.Parse(wgmCreateDto.NumeroGuia))
            // {
            //     logger.LogWarning("El correlativo de la guia de servicio no coincide", "CreateWorkGuideMainEndpoint");
            //     return Results.BadRequest("El correlativo de la guia de servicio no coincide");
            // }

            //validar si el cliente existe
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == wgmCreateDto.CustomerId);
            if (customer == null)
            {
                logger.LogWarning("No se encontró cliente", "CreateWorkGuideMainEndpoint");
                return Results.BadRequest("No se encontró cliente");
            }


            string identifierLog = Guid.NewGuid().ToString();

            DateTime fechaOperacion = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            var workGuideMain = new WorkGuideMain
            {
                SerieGuia = numbersDocument.SerieDoc,
                NumeroGuia = numbersDocument.NumberDoc.ToString(), //wgmCreateDto.NumeroGuia,
                FechaOperacion = fechaOperacion,
                FechaHoraEntrega = fechaOperacion.AddDays(2), // Add 2 days to the current date
                MensajeAlertas = wgmCreateDto.MensajeAlertas,
                Observaciones = wgmCreateDto.Observaciones,
                TipoPago = wgmCreateDto.TipoPago,
                DescripcionPago = wgmCreateDto.DescripcionPago,
                TipoRecepcion = wgmCreateDto.TipoRecepcion,
                DireccionContacto = wgmCreateDto.DireccionContacto,
                TelefonoContacto = wgmCreateDto.TelefonoContacto,
                Total = wgmCreateDto.Total,
                Acuenta = wgmCreateDto.Acuenta,
                Saldo = wgmCreateDto.Saldo,
                CustomerId = wgmCreateDto.CustomerId,
                BranchId = wgmCreateDto.BranchStoreId,

                EstadoPago = wgmCreateDto.EstadoPago,
                FechaPago = wgmCreateDto.EstadoPago == "PA" ? fechaOperacion : null,
                EstadoRegistro = wgmCreateDto.EstadoRegistro,
                EstadoSituacion = wgmCreateDto.EstadoSituacion,
                FechaRecojo = null,
                UserId =  wgmCreateDto.UserId,

                WorkGuideDetails = wgmCreateDto.WorkGuideDetailsDTO.Select(x => new WorkGuideDetail
                {
                    Cant = x.Cant,
                    Precio = x.Precio,
                    Total = x.Total,
                    Observaciones = x.Observaciones,
                    TipoLavado = x.TipoLavado,
                    Ubicacion = x.Ubicacion,
                    EstadoTrabajo = x.EstadoTrabajo,
                    ProductId = x.ProductId,
                    EstadoRegistro = x.EstadoRegistro,
                    EstadoSituacion = x.EstadoSituacion,
                    EstadoPago = x.EstadoPago,
                    Identificador = x.Identificador
                }).ToList()
            };

            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == wgmCreateDto.UserId);
            if (user == null)
            {
                logger.LogInformacion($"Usuario con {wgmCreateDto.UserId} no encontrado");
            }


            // registrar el adelanto en caja
            if (wgmCreateDto.Acuenta > 0)
            {
                var cashBoxMain = await db.CashBoxMains.FirstOrDefaultAsync(c => c.UserId == wgmCreateDto.UserId && c.EstadoRegistro == "A" && c.EstadoCaja == "A");
                if (cashBoxMain == null)
                {
                    logger.LogWarning("No se encontró caja abierta para el usuario", "CreateWorkGuideMainEndpoint");
                    return Results.BadRequest("No se encontró caja abierta para el usuario");
                }
                var cashBoxDetail = new CashBoxDetail
                {
                    TipoComprobante = numbersDocument!.TypeDoc,
                    SerieComprobante = numbersDocument!.SerieDoc,
                    NumComprobante = numbersDocument!.NumberDoc.ToString(),
                    FechaComprobante = fechaOperacion,
                    Importe = 0,
                    Adelanto = wgmCreateDto.Acuenta,
                    TipoPago = wgmCreateDto.TipoPago,
                    DescripcionPago = wgmCreateDto.DescripcionPago,
                    Observaciones = "Adelanto de guia de servicio",
                    EstadoRegistro = "A",
                    CustomerId = wgmCreateDto.CustomerId,
                    CashBoxMainId = cashBoxMain.Id
                };

                await db.CashBoxDetails.AddAsync(cashBoxDetail);

                var dataCaja = new
                {
                    cajaId = cashBoxMain.Id,
                    fecha = fechaOperacion,
                    clienteId = wgmCreateDto.CustomerId,
                    comprobante = numbersDocument!.TypeDoc + " " + numbersDocument!.SerieDoc + " " + numbersDocument!.NumberDoc,
                    tipoPago = wgmCreateDto.TipoPago,
                    monto = wgmCreateDto.Acuenta,
                };

                var cajaString = JsonSerializer.Serialize(dataCaja);
                logger.LogMessageWithEventAndId($"Adelanto de caja, por {user!.UserName}", 1001, identifierLog, cajaString);
            }

            await db.WorkGuideMains.AddAsync(workGuideMain);

            // actualiza el correlativo de la guia de servicio
            numbersDocument.NumberDoc = numbersDocument.NumberDoc + 1;
            db.NumbersDocuments.Update(numbersDocument);
            await db.SaveChangesAsync();
            
            
            //envia a la cola de impresion
            var branch = await db.BranchSales.SingleOrDefaultAsync(x => x.Id == wgmCreateDto.BranchStoreId);
            if (branch == null)
            {
                return Results.Problem("No tiene impresora configurada por defecto esta sucursal", "CreateWorkGuideMainEndpoint");
            }

            var printQueue = new PrintQueue()
            {
                WorkGuideId = workGuideMain.Id,
                PrinterId = branch.DefaultPrinterId,
                Status = PrintStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                DocumentType = numbersDocument.TypeDoc,
                PrintCopy = wgmCreateDto.PrintCopy,
            };
            db.PrintQueues.Add(printQueue);
            await db.SaveChangesAsync();
            await printNotifier.NotifyAsync(branch.DefaultPrinterId, printQueue.Id);

            // Actualiza el historial de prendas frecuentes del cliente. No debe romper el flujo de registro/impresion ya realizado.
            await UpsertGarmentHistoryAsync(db, logger, workGuideMain.CustomerId, workGuideMain.WorkGuideDetails);

            var resp = new WgmCreateResponseDto(
                 workGuideMain.Id,
                 numbersDocument!.TypeDoc,
                 workGuideMain.SerieGuia,
                 workGuideMain.NumeroGuia,
                 workGuideMain.FechaOperacion,
                 workGuideMain.FechaHoraEntrega,
                 workGuideMain.WorkGuideDetails.Select(d => new WgdCreateResponseDto(d.Id, d.ProductId, d.Identificador))
            );

            var dataGuiaLog = new
            {
                idGuia = workGuideMain.Id,
                comprobante = workGuideMain.SerieGuia + " " + workGuideMain.NumeroGuia,
                fechaOperacion = workGuideMain.FechaOperacion,
                clienteId = workGuideMain.CustomerId,
                total = workGuideMain.Total,
                acuenta = workGuideMain.Acuenta,
                saldo = workGuideMain.Saldo,
                detalles = workGuideMain.WorkGuideDetails.Select(x => new
                {
                    productoId = x.ProductId,
                    cantidad = x.Cant,
                    precio = x.Precio,
                    total = x.Total,
                    observaciones = x.Observaciones
                })
            };
            var guiaString = JsonSerializer.Serialize(dataGuiaLog);
            logger.LogMessageWithEventAndId($"Guia creada por {user!.UserName} ", 1001, identifierLog, guiaString);

            var response = new ApiResponse<WgmCreateResponseDto>
            {
                Data = resp,
                Success = true,
                Message = "",
                StatusCode = 200
            };

            return Results.Ok(response);
        });
    }

    private static async Task UpsertGarmentHistoryAsync(RecepcionDbContext db, IAppLogger<string> logger, int customerId, IEnumerable<WorkGuideDetail> details)
    {
        try
        {
            var productIds = details.Select(d => d.ProductId).Distinct().ToList();
            if (productIds.Count == 0)
            {
                return;
            }

            // Los servicios por peso (ej. "Lavado por Kilo") no representan una prenda individual, se excluyen del historial.
            var pesoProductIds = await db.ProdServices
                .Where(p => productIds.Contains(p.Id) && p.IsPeso == "S")
                .Select(p => p.Id)
                .ToListAsync();

            var eligibleGroups = details
                .Where(d => !pesoProductIds.Contains(d.ProductId))
                .GroupBy(d => new { d.ProductId, Normalized = NormalizeDetail(d.Observaciones) })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Normalized,
                    Detail = g.First().Observaciones ?? "",
                    Count = g.Count()
                })
                .ToList();

            if (eligibleGroups.Count == 0)
            {
                return;
            }

            DateTime now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            foreach (var group in eligibleGroups)
            {
                var existing = await db.CustomerGarmentHistories.FirstOrDefaultAsync(h =>
                    h.CustomerId == customerId &&
                    h.ProductId == group.ProductId &&
                    h.NormalizedDetail == group.Normalized);

                if (existing != null)
                {
                    existing.UsageCount += group.Count;
                    existing.LastUsedAt = now;
                    existing.Status = "A"; // revive si estaba depurado
                }
                else
                {
                    db.CustomerGarmentHistories.Add(new CustomerGarmentHistory
                    {
                        CustomerId = customerId,
                        ProductId = group.ProductId,
                        Detail = group.Detail,
                        NormalizedDetail = group.Normalized,
                        UsageCount = group.Count,
                        LastUsedAt = now,
                        CreatedAt = now,
                        Status = "A"
                    });
                }
            }

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning($"No se pudo actualizar el historial de prendas del cliente {customerId}: {ex.Message}", "CreateWorkGuideMainEndpoint");
        }
    }

    private static string NormalizeDetail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"\s+", " ");

        var formD = normalized.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}