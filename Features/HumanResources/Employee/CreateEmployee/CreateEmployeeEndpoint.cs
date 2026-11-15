using FluentValidation;

public static class CreateEmployeeEndpoint
{
    public static RouteGroupBuilder MapCreateEmployee(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (ICreateEmployeeService service, CreateEmployeeRequestDto request, IValidator<CreateEmployeeRequestDto> validator) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var result = await service.CreateEmployeeAsync(request);
            var response = new ApiResponse<EmployeeResponseDto>
            {
                Success = true,
                Data = result,
                StatusCode = StatusCodes.Status201Created,
                Message = "Trabajador dado de alta correctamente"
            };
            return Results.Ok(response);
        })
        .WithName("CreateEmployee")
        .Produces<ApiResponse<EmployeeResponseDto>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization();

        return group;
    }
}
