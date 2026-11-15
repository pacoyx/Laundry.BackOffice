public static class CancelPurchaseInvoiceEndpoint
{
    public static IEndpointRouteBuilder MapCancelPurchaseInvoice(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/{id:int}/cancel", async (int id, ICancelPurchaseInvoiceService service, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var result = await service.CancelPurchaseInvoiceAsync(id, user.GetUserId());
            if (result.NotFound)
            {
                return Results.NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Compra no encontrada."
                });
            }

            if (result.Error is not null)
            {
                return Results.BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Error
                });
            }

            return Results.Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Compra anulada correctamente.",
                StatusCode = StatusCodes.Status200OK,
                Data = new { Id = id, PaymentStatus = result.PaymentStatus }
            });
        })
        .RequireAuthorization()
        .WithName("CancelPurchaseInvoice")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Purchase Invoice");

        return routes;
    }
}
