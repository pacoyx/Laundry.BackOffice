using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateGarmentTicketEndpoint
{
    public static void MapCreateGarmentTicket(this IEndpointRouteBuilder app)
    {
        app.MapPost("/garment-tickets", async (
            ClaimsPrincipal principal,
            CreateGarmentIntakeTicketRequest request,
            IValidator<CreateGarmentIntakeTicketRequest> validator,
            RecepcionDbContext db) =>
        {
            var customerId = principal.GetCustomerId();
            var customerUserId = principal.GetCustomerUserId();
            if (customerId is null || customerUserId is null)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var workerIds = request.Details.Select(d => d.WorkerId).Distinct().ToList();
            var activeWorkers = await db.CompanyWorkers
                .Where(x => x.CustomerId == customerId.Value && workerIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync();

            var missingWorker = workerIds.FirstOrDefault(id => !activeWorkers.Contains(id));
            if (missingWorker != 0)
            {
                return Results.BadRequest(new[] { $"El trabajador '{missingWorker}' no existe o está inactivo." });
            }

            var activePriceList = await db.CustomerPriceLists
                .Include(x => x.Details).ThenInclude(d => d.ServiceItem)
                .FirstOrDefaultAsync(x => x.CustomerId == customerId.Value && x.IsActive);

            if (activePriceList is null)
            {
                return Results.BadRequest(new[] { "La empresa no tiene una tarifa activa. No se puede registrar el ticket." });
            }

            var details = new List<GarmentIntakeTicketDetail>();
            foreach (var detailRequest in request.Details)
            {
                var priceDetail = activePriceList.Details.FirstOrDefault(d => d.ServiceItemId == detailRequest.ServiceItemId && d.UnitPrice > 0);
                if (priceDetail is null)
                {
                    return Results.BadRequest(new[] { $"El servicio '{detailRequest.ServiceItemId}' no está tarifado para esta empresa." });
                }

                details.Add(new GarmentIntakeTicketDetail
                {
                    WorkerId = detailRequest.WorkerId,
                    ServiceItemId = detailRequest.ServiceItemId,
                    Quantity = detailRequest.Quantity,
                    Observations = detailRequest.Observations
                });
            }

            var ticket = new GarmentIntakeTicket
            {
                TicketNumber = Guid.NewGuid().ToString("N"),
                CustomerId = customerId.Value,
                CreatedByCustomerUserId = customerUserId.Value,
                Status = GarmentIntakeTicketStatus.Pending,
                Details = details
            };

            await db.GarmentIntakeTickets.AddAsync(ticket);
            await db.SaveChangesAsync();

            // El ticket usa su propio Id auto-generado como correlativo, igual que PickupGuide.GuideNumber.
            ticket.TicketNumber = $"TK-{ticket.Id:D6}";
            await db.SaveChangesAsync();

            await db.Entry(ticket).Collection(x => x.Details).Query()
                .Include(x => x.Worker)
                .Include(x => x.ServiceItem)
                .LoadAsync();
            await db.Entry(ticket).Reference(x => x.CreatedByCustomerUser).LoadAsync();

            var response = new ApiResponse<GarmentIntakeTicketResponse>
            {
                Success = true,
                Message = "Ticket de ingreso de prendas registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = GarmentTicketsEndpoints.ToResponse(ticket)
            };

            return Results.Created($"/api/customer-portal/garment-tickets/{ticket.Id}", response);
        })
        .WithName("CreateGarmentTicket")
        .Produces<ApiResponse<GarmentIntakeTicketResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireCustomerPermission(CustomerPermissionCodes.GarmentsManage);
    }
}
