using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public static class CreateWebOrderEndpoint
{
    public static void MapCreateWebOrder(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateWebOrderRequest request,
            ClaimsPrincipal principal,
            RecepcionDbContext db) =>
        {
            if (request.Items is null || request.Items.Count == 0)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El pedido debe tener al menos un servicio.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (string.IsNullOrWhiteSpace(request.DeliveryAddress) || string.IsNullOrWhiteSpace(request.ContactPhone))
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "La dirección y el teléfono de contacto son obligatorios.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var (district, priceList) = await ZonePricingResolver.GetActivePriceListForDistrictAsync(db, request.DistrictId);

            if (district is null)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "El distrito indicado no existe.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            if (priceList is null)
            {
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "No hay una lista de precios activa para esa zona.",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            // El front manda un Date de JS, que se serializa como UTC. Si llega con Kind=Utc, se
            // convierte a hora Peru antes de guardar; si llega sin offset, ya se asume hora Peru.
            DateTime? NormalizePeruDate(DateTime? value)
            {
                if (value is null)
                {
                    return null;
                }

                return value.Value.Kind == DateTimeKind.Utc
                    ? TimeZoneInfo.ConvertTimeFromUtc(value.Value, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"))
                    : DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
            }

            var webCustomerId = principal.GetWebCustomerCustomerId();
            Customer customer;

            if (webCustomerId.HasValue)
            {
                var loggedInCustomer = await db.Customers.FirstOrDefaultAsync(x => x.Id == webCustomerId.Value);
                if (loggedInCustomer is null)
                {
                    return Results.Unauthorized();
                }

                customer = loggedInCustomer;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.GuestName) || string.IsNullOrWhiteSpace(request.GuestPhone))
                {
                    return Results.BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Si no tienes cuenta, ingresa tu nombre y celular para continuar.",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                var existingCustomer = await db.Customers.FirstOrDefaultAsync(x => x.Phone == request.GuestPhone);

                if (existingCustomer != null)
                {
                    customer = existingCustomer;
                }
                else
                {
                    customer = new Customer
                    {
                        FirtsName = request.GuestName!,
                        Phone = request.GuestPhone,
                        DistrictId = request.DistrictId,
                        CustomerType = CustomerType.Individual,
                        Origin = CustomerOrigin.WebLanding,
                        Status = "A",
                        IsActive = true
                    };

                    db.Customers.Add(customer);
                    await db.SaveChangesAsync();
                }
            }

            var details = new List<WebOrderDetail>();
            decimal total = 0;

            foreach (var item in request.Items)
            {
                var priceDetail = priceList.Details.FirstOrDefault(d => d.ServiceItemId == item.ServiceItemId);
                if (priceDetail is null)
                {
                    return Results.BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"El servicio '{item.ServiceItemId}' no está disponible en tu zona.",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                if (item.Quantity <= 0)
                {
                    return Results.BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "La cantidad de cada servicio debe ser mayor a cero.",
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                var lineTotal = priceDetail.UnitPrice * item.Quantity;
                total += lineTotal;

                details.Add(new WebOrderDetail
                {
                    ServiceItemId = item.ServiceItemId,
                    Quantity = item.Quantity,
                    UnitPrice = priceDetail.UnitPrice,
                    Total = lineTotal
                });
            }

            var webOrder = new WebOrder
            {
                CustomerId = customer.Id,
                Status = WebOrderStatus.Pendiente,
                DeliveryAddress = request.DeliveryAddress,
                DistrictId = request.DistrictId,
                ContactPhone = request.ContactPhone,
                ZonePriceListId = priceList.Id,
                RequestedPickupDate = NormalizePeruDate(request.RequestedPickupDate),
                AdditionalItemsNote = request.AdditionalItemsNote,
                Total = total,
                CreatedAt = now,
                Details = details
            };

            db.WebOrders.Add(webOrder);
            await db.SaveChangesAsync();

            webOrder.OrderNumber = $"WEB-{webOrder.Id:D6}";
            await db.SaveChangesAsync();

            var response = new WebOrderResponse(
                webOrder.Id,
                webOrder.OrderNumber,
                webOrder.Status,
                webOrder.DeliveryAddress,
                webOrder.DistrictId,
                webOrder.ContactPhone,
                webOrder.RequestedPickupDate,
                webOrder.ScheduledPickupDate,
                webOrder.AdditionalItemsNote,
                webOrder.Total,
                webOrder.CreatedAt,
                null,
                null,
                details.Select(d => new WebOrderDetailResponse(
                    d.ServiceItemId,
                    priceList.Details.First(p => p.ServiceItemId == d.ServiceItemId).ServiceItem.Name,
                    d.Quantity,
                    d.UnitPrice,
                    d.Total)).ToList());

            return Results.Ok(new ApiResponse<WebOrderResponse>
            {
                Success = true,
                Message = "Pedido registrado. Quedará pendiente de confirmación.",
                StatusCode = StatusCodes.Status200OK,
                Data = response
            });
        })
        .WithName("CreateWebOrder")
        .WithTags("WebOrders")
        .Produces<ApiResponse<WebOrderResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}
