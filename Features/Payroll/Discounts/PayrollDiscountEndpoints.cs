using FluentValidation;

public static class PayrollDiscountEndpoints
{
    public static RouteGroupBuilder MapPayrollDiscounts(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:long}/discounts", async (long id, long? employeeId, IPayrollDiscountService service) =>
        {
            var result = await service.ListAsync(id, employeeId);
            if (result == null)
            {
                return Results.NotFound(new ApiResponse<IEnumerable<PayrollDiscountDto>>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Periodo de planilla con ID {id} no encontrado."
                });
            }

            return Results.Ok(new ApiResponse<IEnumerable<PayrollDiscountDto>>
            {
                Success = true,
                Data = result,
                StatusCode = StatusCodes.Status200OK,
                Message = "Descuentos obtenidos correctamente"
            });
        })
        .WithName("GetPayrollDiscounts")
        .Produces<ApiResponse<IEnumerable<PayrollDiscountDto>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        group.MapPost("/{id:long}/discounts", async (long id, CreatePayrollDiscountRequestDto request,
            IValidator<CreatePayrollDiscountRequestDto> validator, IPayrollDiscountService service) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var result = await service.AddAsync(id, request);
            return ToHttpResult(id, result, "Descuento registrado correctamente");
        })
        .WithName("CreatePayrollDiscount")
        .Produces<ApiResponse<PayrollDiscountChangeResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        group.MapDelete("/{id:long}/discounts/{discountId:long}", async (long id, long discountId, IPayrollDiscountService service) =>
        {
            var result = await service.DeleteAsync(id, discountId);
            return ToHttpResult(id, result, "Descuento eliminado correctamente");
        })
        .WithName("DeletePayrollDiscount")
        .Produces<ApiResponse<PayrollDiscountChangeResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }

    private static IResult ToHttpResult(long periodId, PayrollDiscountResult result, string okMessage) => result.Status switch
    {
        PayrollDiscountStatus.PeriodNotFound => NotFound($"Periodo de planilla con ID {periodId} no encontrado."),
        PayrollDiscountStatus.EmployeeNotFound => NotFound("Trabajador no encontrado."),
        PayrollDiscountStatus.DiscountNotFound => NotFound("Descuento no encontrado."),
        PayrollDiscountStatus.PeriodClosed => Results.BadRequest(new[] { "El periodo está cerrado; no se pueden modificar sus descuentos." }),
        PayrollDiscountStatus.EmployeeAdHonorem => Results.BadRequest(new[] { "El trabajador es ad honorem: no tiene planilla, no se le pueden registrar descuentos." }),
        PayrollDiscountStatus.NegativeNet => Results.BadRequest(new[] { result.Message ?? "El descuento deja el neto a pagar en negativo." }),
        _ => Results.Ok(new ApiResponse<PayrollDiscountChangeResponseDto>
        {
            Success = true,
            Data = result.Data,
            StatusCode = StatusCodes.Status200OK,
            Message = okMessage
        })
    };

    private static IResult NotFound(string message) => Results.NotFound(new ApiResponse<PayrollDiscountChangeResponseDto>
    {
        Success = false,
        StatusCode = StatusCodes.Status404NotFound,
        Message = message
    });
}
