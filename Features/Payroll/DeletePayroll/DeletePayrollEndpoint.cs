public static class DeletePayrollEndpoint
{
    public static RouteGroupBuilder MapDeletePayroll(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}/payroll", async (long id, IDeletePayrollService service) =>
        {
            var result = await service.DeletePayrollAsync(id);

            return result.Status switch
            {
                DeletePayrollStatus.PeriodNotFound => Results.NotFound(new ApiResponse<DeletePayrollResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Periodo de planilla con ID {id} no encontrado."
                }),
                DeletePayrollStatus.PeriodClosed => Results.BadRequest(new[] { "No se puede eliminar la planilla de un periodo cerrado." }),
                DeletePayrollStatus.NothingToDelete => Results.BadRequest(new[] { "El periodo no tiene planillas generadas." }),
                _ => Results.Ok(new ApiResponse<DeletePayrollResponseDto>
                {
                    Success = true,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Planilla eliminada correctamente"
                })
            };
        })
        .WithName("DeletePayroll")
        .Produces<ApiResponse<DeletePayrollResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
