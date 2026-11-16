using FluentValidation;

public static class UpdateLoanEndpoint
{
    /// <summary>PUT /{id}: edita un préstamo/adelanto sin cuotas cobradas y regenera su cronograma.</summary>
    public static RouteGroupBuilder MapUpdateLoan(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:long}", async (long id, IUpdateLoanService service, RegisterLoanRequestDto request, IValidator<RegisterLoanRequestDto> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var result = await service.UpdateLoanAsync(id, request);
            return result.Status switch
            {
                UpdateLoanStatus.LoanNotFound => Results.NotFound(new ApiResponse<EmployeeLoanResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Préstamo o adelanto con ID {id} no encontrado."
                }),
                UpdateLoanStatus.HasCollectedInstallments => Results.BadRequest(new[] { LoanModificationGuard.HasCollectedInstallmentsMessage }),
                UpdateLoanStatus.EmployeeAdHonorem => Results.BadRequest(new[] { "El trabajador es ad honorem: no se le pueden registrar préstamos ni adelantos porque no tiene planilla." }),
                _ => Results.Ok(new ApiResponse<EmployeeLoanResponseDto>
                {
                    Success = true,
                    Data = result.Data!,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Préstamo/adelanto actualizado correctamente"
                })
            };
        })
        .WithName("UpdateLoan")
        .Produces<ApiResponse<EmployeeLoanResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
