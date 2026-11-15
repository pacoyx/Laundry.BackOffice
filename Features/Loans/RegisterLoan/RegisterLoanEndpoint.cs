using FluentValidation;

public static class RegisterLoanEndpoint
{
    public static RouteGroupBuilder MapRegisterLoan(this RouteGroupBuilder group)
    {
        group.MapPost("/{employeeId:long}/loans", async (long employeeId, IRegisterLoanService service, RegisterLoanRequestDto request, IValidator<RegisterLoanRequestDto> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var result = await service.RegisterLoanAsync(employeeId, LoanType.Loan, request);
            if (result == null)
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
                Data = result,
                StatusCode = StatusCodes.Status201Created,
                Message = "Préstamo registrado correctamente"
            });
        })
        .WithName("RegisterLoan")
        .Produces<ApiResponse<EmployeeLoanResponseDto>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
