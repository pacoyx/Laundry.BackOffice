public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProduct(this IEndpointRouteBuilder group)
    {
        group.MapPost("/", async (CreateProductRequestDto request, ICreateProductService service) =>
        {
            try
            {
                var result = await service.CreateProductAsync(request);
                var response = new ApiResponse<CreateProductResponseDto>
                {
                    Success = true,
                    Data = result,
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Product created successfully"
                };
                return Results.Ok(response);
            }
            catch (InventoryException ex)
            {
                return Results.BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                });
            }
        })
        .WithName("CreateProduct")
        .Produces<CreateProductResponseDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("Products");

        return group;
    }
}