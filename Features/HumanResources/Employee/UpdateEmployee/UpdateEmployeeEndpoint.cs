using FluentValidation;

public static class UpdateEmployeeEndpoint
{
    public static RouteGroupBuilder MapUpdateEmployee(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:long}", async (long id, IUpdateEmployeeService service, UpdateEmployeeRequestDto request, IValidator<UpdateEmployeeRequestDto> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var result = await service.UpdateEmployeeAsync(id, request);
            if (result == null)
            {
                return Results.NotFound(new ApiResponse<EmployeeResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Trabajador con ID {id} no encontrado."
                });
            }

            return Results.Ok(new ApiResponse<EmployeeResponseDto>
            {
                Success = true,
                Data = result,
                StatusCode = StatusCodes.Status200OK,
                Message = "Trabajador actualizado correctamente"
            });
        })
        .WithName("UpdateEmployee")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces<ApiResponse<EmployeeResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
