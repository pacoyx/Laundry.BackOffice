using Microsoft.EntityFrameworkCore;

public static class ReturnUnwashedClothesEndpoint
{
    public static void MapReturnUnwashedClothes(this IEndpointRouteBuilder app)
    {
        app.MapPut("/returnUnwashedClothes/{id}", async (RecepcionDbContext db, int id, RequestDevolucionDataDto request, IAppLogger<string> logger) =>
        {
            var workGuideItem = await db.WorkGuideDetails.FindAsync(id);
            if (workGuideItem == null)
            {
                logger.LogWarning("Item no encontrado", "ReturnUnwashedClothesEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "Item no encontrado",
                    StatusCode = 404,
                    Success = false
                };
                return Results.NotFound(responseValidation);
            }

            if (workGuideItem.EstadoSituacion == "D")
            {
                logger.LogWarning("El item ya se encuentra devuelto", "ReturnUnwashedClothesEndpoint");
                var responseValidation = new ApiResponse<string>()
                {
                    Data = "",
                    Message = "El item ya se encuentra devuelto",
                    StatusCode = 400,
                    Success = false
                };
                return Results.BadRequest(responseValidation);
            }


            DateTime fechaProceso = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            workGuideItem.EstadoSituacion = "D";
            workGuideItem.FechaDevolucion = fechaProceso;


            // si el monto es mayor a 0 se registra en ExpenseBox
            if (request.DevolverEfectivo && request.Monto > 0)
            {
                // buscar guia de trabajo por id del item y resta el monto del campos saldo
                var workGuide = await db.WorkGuideMains.FindAsync(workGuideItem.WorkGuideMainId);
                if (workGuide == null)
                {
                    logger.LogWarning("No se encontro guia de trabajo", "ReturnUnwashedClothesEndpoint");
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
                if (workGuide.Saldo < 0)
                {
                    workGuide.Saldo = 0;
                }

                // obtener el id de caja principal por el iduser
                var cashBoxMain = await db.CashBoxMains.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.EstadoRegistro == "A" && x.EstadoCaja == "A");
                if (cashBoxMain == null)
                {
                    logger.LogWarning("No se encontro caja abierta para el usuario", "ReturnUnwashedClothesEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "No se encontro caja abierta para el usuario",
                        StatusCode = 404,
                        Success = false
                    };
                    return Results.NotFound(responseValidation);
                }


                var customer = await db.Customers.FindAsync(workGuide.CustomerId);
                if (customer == null)
                {
                    logger.LogWarning("No se encontro el cliente", "ReturnUnwashedClothesEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "No se encontro el cliente",
                        StatusCode = 404,
                        Success = false
                    };
                    return Results.NotFound(responseValidation);
                }

                var product = await db.ProdServices.FindAsync(workGuideItem.ProductId);
                if (product == null)
                {
                    logger.LogWarning("No se encontro el producto", "ReturnUnwashedClothesEndpoint");
                    var responseValidation = new ApiResponse<string>()
                    {
                        Data = "",
                        Message = "No se encontro el producto",
                        StatusCode = 404,
                        Success = false
                    };
                    return Results.NotFound(responseValidation);
                }

                // registrar devolucion de efectivo en ExpenseBox
                var expenseBox = new ExpenseBox()
                {
                    CategoryGasto = "Devolucion de ropa sin lavar",
                    PersonalAutoriza = "Sistema",
                    FechaGasto = fechaProceso,
                    Importe = (decimal)request.Monto,
                    DetallesEgreso = customer.FirtsName + " " + customer.LastName + " | Guia: " + workGuide.SerieGuia + "-" + workGuide.NumeroGuia + " | Producto: " + product.Description,
                    EstadoRegistro = "A",
                    UserId = request.UserId,
                    CashBoxMainId = cashBoxMain.Id
                };
                db.ExpenseBoxMains.Add(expenseBox);
            }

            await db.SaveChangesAsync();
            var response = new ApiResponse<DateTime?>()
            {
                Data = workGuideItem.FechaDevolucion,
                Message = "El item fue actualizado como DEVUELTO con exito",
                Success = true,
                StatusCode = 200
            };

            return Results.Ok(response);
        }).RequirePermission(PermissionCodes.ReceptionRetiro);
    }
}