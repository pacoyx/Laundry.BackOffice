public static class UpdateProductEndpoint
{
    public static void MapUpdateProduct(this IEndpointRouteBuilder app)
    {
        app.MapPut("/", async (UpdateProductRequestDto request, IUpdateProductService updateProductService) =>
        {
            var result = await updateProductService.UpdateProductAsync(request);
            if (result == UpdateProductService.NotFoundResult)
            {
                return Results.NotFound(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = result,
                    Data = string.Empty
                });
            }

            if (result != "OK")
            {
                // Regla de negocio (código duplicado, producto con existencias, etc.).
                return Results.BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = result,
                    Data = string.Empty
                });
            }

            var response = new ApiResponse<string>
            {
                Success = true,
                Data = result,
                StatusCode = StatusCodes.Status200OK,
                Message = "Product updated successfully"
            };

            return Results.Ok(response);
        })
        .WithName("UpdateProduct")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Products");
    }
}