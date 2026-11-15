using System.Security.Claims;

public static class CreatePurchaseInvoiceEndpoint
{
    public static void MapCreatePurchaseInvoice(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/", async (ICreatePurchaseInvoiceService service, PurchaseInvoiceRequestDto request, ClaimsPrincipal user) =>
        {
            try
            {
                var responseInvoice = await service.CreatePurchaseInvoiceAsync(request, user.GetUserId());
                var response = new ApiResponse<PurchaseInvoiceResponseDto>
                {
                    Success = true,
                    Data = responseInvoice,
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Purchase invoice created successfully."
                };

                return Results.Ok(response);
            }
            catch (InventoryException ex)
            {
                // Error de inventario (almacén inválido, producto que no controla stock, etc.): la compra no se guardó.
                return Results.BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                });
            }
        })
        .WithName("CreatePurchaseInvoice")
        .Produces<PurchaseInvoiceResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("Purchase Invoice");
    }
}
