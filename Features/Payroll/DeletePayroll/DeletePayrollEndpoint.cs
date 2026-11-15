public static class DeletePayrollEndpoint
{
    /// <summary>DELETE /{id}/payroll: elimina solo las planillas generadas del periodo (para regenerar).</summary>
    public static RouteGroupBuilder MapDeletePayroll(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}/payroll", async (long id, IDeletePayrollService service) =>
            ToHttpResult(id, await service.DeletePayrollAsync(id), "Planilla eliminada correctamente"))
        .WithName("DeletePayroll")
        .Produces<ApiResponse<DeletePayrollResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }

    /// <summary>DELETE /{id}: elimina el periodo completo (planillas, descuentos y periodo).</summary>
    public static RouteGroupBuilder MapDeletePayrollPeriod(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}", async (long id, IDeletePayrollService service) =>
            ToHttpResult(id, await service.DeletePeriodAsync(id), "Periodo de planilla eliminado correctamente"))
        .WithName("DeletePayrollPeriod")
        .Produces<ApiResponse<DeletePayrollResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }

    private static IResult ToHttpResult(long id, DeletePayrollResult result, string okMessage) => result.Status switch
    {
        DeletePayrollStatus.PeriodNotFound => Results.NotFound(new ApiResponse<DeletePayrollResponseDto>
        {
            Success = false,
            StatusCode = StatusCodes.Status404NotFound,
            Message = $"Periodo de planilla con ID {id} no encontrado."
        }),
        DeletePayrollStatus.PeriodClosed => Results.BadRequest(new[] { "El periodo está cerrado; no se puede eliminar." }),
        DeletePayrollStatus.NothingToDelete => Results.BadRequest(new[] { "El periodo no tiene planillas generadas." }),
        _ => Results.Ok(new ApiResponse<DeletePayrollResponseDto>
        {
            Success = true,
            Data = result.Data,
            StatusCode = StatusCodes.Status200OK,
            Message = okMessage
        })
    };
}
