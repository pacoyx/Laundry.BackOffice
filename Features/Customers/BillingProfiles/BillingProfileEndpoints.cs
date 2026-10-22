using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class BillingProfileEndpoints
{
    public static void MapBillingProfiles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}/billing-profile", async (int id, RecepcionDbContext db) =>
        {
            var profile = await db.CustomerBillingProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustomerId == id);

            if (profile is null)
            {
                return Results.NotFound(new ApiResponse<CustomerBillingProfileResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "El cliente no tiene un perfil de facturación registrado."
                });
            }

            return Results.Ok(new ApiResponse<CustomerBillingProfileResponseDto>
            {
                Success = true,
                Message = "Perfil de facturación obtenido correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = ToResponse(profile)
            });
        })
        .WithName("GetCustomerBillingProfile")
        .Produces<ApiResponse<CustomerBillingProfileResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        app.MapPost("/{id:int}/billing-profile", async (
            int id,
            CustomerBillingProfileRequestDto request,
            IValidator<CustomerBillingProfileRequestDto> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var customer = await db.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (customer is null)
            {
                return Results.NotFound("Cliente no encontrado.");
            }

            if (customer.CustomerType != CustomerType.Company)
            {
                return Results.BadRequest(new[] { "Solo los clientes de tipo Empresa pueden tener perfil de facturación." });
            }

            var exists = await db.CustomerBillingProfiles.AnyAsync(x => x.CustomerId == id);
            if (exists)
            {
                return Results.BadRequest(new[] { "El cliente ya tiene un perfil de facturación registrado. Use PUT para actualizarlo." });
            }

            var profile = new CustomerBillingProfile
            {
                CustomerId = id,
                TaxId = request.TaxId,
                BusinessName = request.BusinessName,
                CommercialName = request.CommercialName,
                FiscalAddress = request.FiscalAddress,
                BillingEmail = request.BillingEmail,
                PaymentTermDays = request.PaymentTermDays,
                CreditLimit = request.CreditLimit,
                IsCreditEnabled = request.IsCreditEnabled
            };

            await db.CustomerBillingProfiles.AddAsync(profile);
            await db.SaveChangesAsync();

            var response = new ApiResponse<CustomerBillingProfileResponseDto>
            {
                Success = true,
                Message = "Perfil de facturación registrado correctamente.",
                StatusCode = StatusCodes.Status201Created,
                Data = ToResponse(profile)
            };

            return Results.Created($"/api/customers/{id}/billing-profile", response);
        })
        .WithName("CreateCustomerBillingProfile")
        .Produces<ApiResponse<CustomerBillingProfileResponseDto>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        app.MapPut("/{id:int}/billing-profile", async (
            int id,
            CustomerBillingProfileRequestDto request,
            IValidator<CustomerBillingProfileRequestDto> validator,
            RecepcionDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var profile = await db.CustomerBillingProfiles.FirstOrDefaultAsync(x => x.CustomerId == id);
            if (profile is null)
            {
                return Results.NotFound("El cliente no tiene un perfil de facturación registrado.");
            }

            profile.TaxId = request.TaxId;
            profile.BusinessName = request.BusinessName;
            profile.CommercialName = request.CommercialName;
            profile.FiscalAddress = request.FiscalAddress;
            profile.BillingEmail = request.BillingEmail;
            profile.PaymentTermDays = request.PaymentTermDays;
            profile.CreditLimit = request.CreditLimit;
            profile.IsCreditEnabled = request.IsCreditEnabled;

            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse<CustomerBillingProfileResponseDto>
            {
                Success = true,
                Message = "Perfil de facturación actualizado correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = ToResponse(profile)
            });
        })
        .WithName("UpdateCustomerBillingProfile")
        .Produces<ApiResponse<CustomerBillingProfileResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        app.MapUpdateBillingProfileReminders();
    }

    private static CustomerBillingProfileResponseDto ToResponse(CustomerBillingProfile profile) => new(
        profile.CustomerId,
        profile.TaxId,
        profile.BusinessName,
        profile.CommercialName,
        profile.FiscalAddress,
        profile.BillingEmail,
        profile.PaymentTermDays,
        profile.CreditLimit,
        profile.IsCreditEnabled,
        profile.RemindersEnabled,
        profile.RemindersSuppressedReason);
}
