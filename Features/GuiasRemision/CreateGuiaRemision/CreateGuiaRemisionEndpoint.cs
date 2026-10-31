using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class CreateGuiaRemisionEndpoint
{
    private const string TypeProcess = "GUIAR"; // max 5 caracteres (NumbersDocument.TypeProcess)

    public static void MapCreateGuiaRemision(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateGuiaRemisionRequest request,
            IValidator<CreateGuiaRemisionRequest> validator,
            RecepcionDbContext db,
            ICompanyProfileService companyProfileService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var invoice = await db.Invoices
                .Include(x => x.Customer).ThenInclude(c => c.BillingProfile)
                .FirstOrDefaultAsync(x => x.Serie == request.Serie && x.Numero == request.Numero);

            if (invoice is null)
            {
                return Results.NotFound("Factura no encontrada.");
            }

            var yaTieneGuia = await db.GuiasRemision.AnyAsync(x => x.InvoiceId == invoice.Id);
            if (yaTieneGuia)
            {
                return Results.BadRequest(new[] { "Esta factura ya tiene una guía de remisión registrada." });
            }

            if (request.VehicleId is not null)
            {
                var vehicleExists = await db.Vehicles.AnyAsync(x => x.Id == request.VehicleId);
                if (!vehicleExists)
                {
                    return Results.BadRequest(new[] { "El vehículo indicado no existe." });
                }
            }

            var numbersDocument = await db.NumbersDocuments
                .FirstOrDefaultAsync(nd => nd.BranchId == invoice.BranchId && nd.TypeProcess == TypeProcess);

            if (numbersDocument is null)
            {
                return Results.BadRequest(new[]
                {
                    "No se encontró correlativo de guía de remisión configurado para esta sucursal."
                });
            }

            var originAddress = request.OriginAddress;
            if (string.IsNullOrWhiteSpace(originAddress))
            {
                var company = await companyProfileService.GetCompanyAsync();
                originAddress = company?.Address ?? string.Empty;
            }

            var deliveryAddress = request.DeliveryAddress;
            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                deliveryAddress = GetCustomerAddress(invoice.Customer);
            }

            var guia = new GuiaRemision
            {
                InvoiceId = invoice.Id,
                Serie = numbersDocument.SerieDoc,
                Numero = numbersDocument.NumberDoc.ToString(),
                FechaEmision = DateTime.UtcNow,
                FechaTraslado = request.FechaTraslado,
                MotivoTraslado = string.IsNullOrWhiteSpace(request.MotivoTraslado) ? "Venta" : request.MotivoTraslado,
                DriverName = request.DriverName,
                DriverDni = request.DriverDni,
                OriginAddress = originAddress,
                DeliveryAddress = deliveryAddress,
                VehicleId = request.VehicleId,
                CreatedAt = DateTime.UtcNow
            };

            numbersDocument.NumberDoc += 1;
            db.NumbersDocuments.Update(numbersDocument);

            await db.GuiasRemision.AddAsync(guia);
            await db.SaveChangesAsync();

            guia.Invoice = invoice;
            if (guia.VehicleId is not null)
            {
                guia.Vehicle = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == guia.VehicleId);
            }

            var response = new ApiResponse<GuiaRemisionResponse>
            {
                Success = true,
                Message = "Guía de remisión creada correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = GuiasRemisionEndpoints.ToResponse(guia)
            };

            return Results.Created($"/api/guias-remision/{guia.Id}", response);
        })
        .WithName("CreateGuiaRemision")
        .Produces<ApiResponse<GuiaRemisionResponse>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static string GetCustomerAddress(Customer customer) =>
        customer.CustomerType == CustomerType.Company && customer.BillingProfile != null
            ? customer.BillingProfile.FiscalAddress
            : customer.Address ?? string.Empty;
}
