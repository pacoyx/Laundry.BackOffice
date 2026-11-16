public static class DeleteLoanEndpoint
{
    /// <summary>DELETE /{id}: elimina un préstamo/adelanto sin cuotas cobradas junto con sus cuotas.</summary>
    public static RouteGroupBuilder MapDeleteLoan(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:long}", async (long id, IDeleteLoanService service) =>
        {
            var result = await service.DeleteLoanAsync(id);
            return result.Status switch
            {
                DeleteLoanStatus.LoanNotFound => Results.NotFound(new ApiResponse<DeleteLoanResponseDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Préstamo o adelanto con ID {id} no encontrado."
                }),
                DeleteLoanStatus.HasCollectedInstallments => Results.BadRequest(new[] { LoanModificationGuard.HasCollectedInstallmentsMessage }),
                _ => Results.Ok(new ApiResponse<DeleteLoanResponseDto>
                {
                    Success = true,
                    Data = result.Data!,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Préstamo/adelanto eliminado correctamente"
                })
            };
        })
        .WithName("DeleteLoan")
        .Produces<ApiResponse<DeleteLoanResponseDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return group;
    }
}
