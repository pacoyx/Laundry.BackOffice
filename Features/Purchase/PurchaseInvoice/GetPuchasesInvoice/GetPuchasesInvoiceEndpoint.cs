using Microsoft.AspNetCore.Mvc;

public static class GetPuchasesInvoiceEndpoint
{
    public static IEndpointRouteBuilder MapGetPuchasesInvoice(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", async (
            [FromQuery] int? supplierId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? status,
            [FromQuery] string? invoiceNumber,
            IGetPuchasesInvoiceService service) =>
        {
            var invoices = await service.GetPurchasesInvoiceAsync(supplierId, dateFrom, dateTo, status, invoiceNumber);
            var response = new ApiResponse<IEnumerable<GetPurchaseInvoiceResponseDto>>
            {
                Success = true,
                Data = invoices,
                Message = "Invoices retrieved successfully."
            };

            return Results.Ok(response);
        })
        .WithName("GetPurchasesInvoice")
        .Produces<IEnumerable<GetPurchaseInvoiceResponseDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("Purchase Invoice");

        routes.MapGet("/{id:int}", async (int id, IGetPuchasesInvoiceService service) =>
        {
            var invoice = await service.GetPurchaseInvoiceAsync(id);
            if (invoice == null)
            {
                return Results.NotFound(new ApiResponse<GetPurchaseInvoiceByIdResponseDto>
                {
                    Success = false,
                    Message = "Invoice not found."
                });
            }

            var response = new ApiResponse<GetPurchaseInvoiceByIdResponseDto>
            {
                Success = true,
                Data = invoice,
                Message = "Invoice retrieved successfully."
            };

            return Results.Ok(response);
        })
        .WithName("GetPurchaseInvoiceById")
        .Produces<GetPurchaseInvoiceByIdResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("Purchase Invoice");

        return routes;
    }
}