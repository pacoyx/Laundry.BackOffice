using Microsoft.EntityFrameworkCore;

public class UpdateProductService : IUpdateProductService
{
    public const string NotFoundResult = "Product not found";

    private readonly RecepcionDbContext _context;
    public IAppLogger<UpdateProductService> Logger { get; }

    public UpdateProductService(RecepcionDbContext context, IAppLogger<UpdateProductService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<string> UpdateProductAsync(UpdateProductRequestDto request)
    {
        var product = await _context.Products.FindAsync(request.Id);
        if (product == null)
        {
            Logger.LogWarning($"Product with id {request.Id} not found.");
            return NotFoundResult;
        }

        var code = ProductCodeRules.Normalize(request.Code);
        if (code is not null && await _context.Products.AnyAsync(p => p.Code == code && p.Id != request.Id))
        {
            return $"Ya existe un producto con el código {code}.";
        }

        // Dejar de controlar stock solo si no quedan existencias (si no, el saldo quedaría huérfano).
        if (product.TracksStock && !request.TracksStock
            && await _context.ProductStocks.AnyAsync(s => s.ProductId == product.Id && s.Quantity != 0))
        {
            return "No se puede desactivar el control de stock de un producto que tiene existencias.";
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.UnitMeasurementId = request.UnitMeasurementId;
        product.CategoryProdId = request.CategoryProdId;
        product.Status = request.Status;
        product.Code = code;
        product.TracksStock = request.TracksStock;
        product.MinStock = request.MinStock < 0 ? 0 : request.MinStock;
        product.IsSellable = request.IsSellable;

        await _context.SaveChangesAsync();
        return "OK";
    }
}
