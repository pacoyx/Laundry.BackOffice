using FluentValidation;

public static class RegisterSalaryAdvanceEndpoint
{
    public static RouteGroupBuilder MapRegisterSalaryAdvance(this RouteGroupBuilder group)
    {
        group.MapPost("/{employeeId:long}/salary-advances", async (long employeeId, IRegisterLoanService service, RegisterLoanRequestDto request, IValidator<RegisterLoanRequestDto> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var result = await service.RegisterLoanAsync(employeeId, LoanType.SalaryAdvance, request);
            if (result.Status == RegisterLoanStatus.EmployeeAdHonorem)
            {
                return Results.BadRequest(new[] { "El trabajador es ad honorem: no se le pueden registrar préstamos ni adelantos porque no tiene planilla." });
            }

            if (result.Status == RegisterLoanStatus.EmployeeNotFound)
            {
                return Results.NotFound(new ApiResponse<EmployeeLoanResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Trabajador con ID {employeeId} no encontrado."
                });
            }

            return Results.Ok(new ApiResponse<EmployeeLoanResponseDto>
            {
                Success = true,
                Data = result.Data,
                StatusCode = StatusCodes.Status201Created,
                Message = "Adelanto registrado correctamente"
            });
        })
        .WithName("RegisterSalaryAdvance")
        .Produces<ApiResponse<EmployeeLoanResponseDto>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
