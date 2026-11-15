public record GetProductsResponsePaginatorDto(
    int TotalCount,
    List<GetProductsResponseDto> Products
    );

public record GetProductsResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    /// <summary>Stock total en almacenes activos (calculado desde los movimientos de inventario).</summary>
    public decimal Stock { get; set; }
    public int UnitMeasurementId { get; set; }
    public int CategoryProdId { get; set; }
    public string Status { get; set; } = "A";
    public string? Code { get; set; }
    public bool TracksStock { get; set; }
    public decimal MinStock { get; set; }
    public bool IsSellable { get; set; }
}

public record GetProductsByPatronResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int UnitMeasurementId { get; set; }
    public string UnitMeasurementDescription { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool TracksStock { get; set; }
    /// <summary>Stock total en almacenes activos.</summary>
    public decimal Stock { get; set; }
}
