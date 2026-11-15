public record UpdateProductRequestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    /// <summary>Ignorado: el stock solo cambia con movimientos de inventario.</summary>
    public decimal Stock { get; set; }
    public int UnitMeasurementId { get; set; }
    public int CategoryProdId { get; set; }
    public string Status { get; set; } = "";
    public string? Code { get; set; }
    public bool TracksStock { get; set; } = true;
    public decimal MinStock { get; set; }
    public bool IsSellable { get; set; }
}
