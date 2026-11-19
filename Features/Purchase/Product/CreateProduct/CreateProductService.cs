using Microsoft.EntityFrameworkCore;

public class CreateProductService : ICreateProductService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<CreateProductService> Logger { get; }

    public CreateProductService(RecepcionDbContext context, IAppLogger<CreateProductService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<CreateProductResponseDto> CreateProductAsync(CreateProductRequestDto request)
    {
        var code = ProductCodeRules.Normalize(request.Code);
        if (code is not null && await _context.Products.AnyAsync(p => p.Code == code))
        {
            throw new InventoryException($"Ya existe un producto con el código {code}.");
        }

        // El stock no se recibe: un producto nuevo empieza sin existencias y se carga con un movimiento
        // de "Inventario inicial" o con una compra.
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            UnitMeasurementId = request.UnitMeasurementId,
            CategoryProdId = request.CategoryProdId,
            Status = request.Status,
            Code = code,
            TracksStock = request.TracksStock,
            MinStock = request.MinStock < 0 ? 0 : request.MinStock,
            IsSellable = request.IsSellable
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new CreateProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = 0,
            UnitMeasurementId = product.UnitMeasurementId,
            CategoryProdId = product.CategoryProdId,
            Status = product.Status,
            Code = product.Code,
            TracksStock = product.TracksStock,
            MinStock = product.MinStock,
            IsSellable = product.IsSellable
        };
    }
}

public static class ProductCodeRules
{
    /// <summary>Código en mayúsculas sin espacios extremos; vacío se guarda como null (el índice único lo ignora).</summary>
    public static string? Normalize(string? code)
    {
        var trimmed = code?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
