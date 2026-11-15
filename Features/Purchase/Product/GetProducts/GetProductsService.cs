using Microsoft.EntityFrameworkCore;

public class GetProductsService : IGetProductsService
{

    private readonly RecepcionDbContext _context;
    public IAppLogger<GetServicesAccessFastService> Logger { get; }

    public GetProductsService(RecepcionDbContext context, IAppLogger<GetServicesAccessFastService> logger)
    {
        _context = context;
        Logger = logger;
    }

    /// <summary>Proyección común: el stock es la suma de los saldos en almacenes activos.</summary>
    private IQueryable<GetProductsResponseDto> Project(IQueryable<Product> query) =>
        query.Select(p => new GetProductsResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = _context.ProductStocks
                .Where(s => s.ProductId == p.Id && s.Warehouse!.IsActive)
                .Sum(s => (decimal?)s.Quantity) ?? 0,
            UnitMeasurementId = p.UnitMeasurementId,
            CategoryProdId = p.CategoryProdId,
            Status = p.Status,
            Code = p.Code,
            TracksStock = p.TracksStock,
            MinStock = p.MinStock,
            IsSellable = p.IsSellable
        });

    public async Task<GetProductsResponseDto?> GetProductAsync(int id)
    {
        var product = await Project(_context.Products.AsNoTracking().Where(p => p.Id == id)).FirstOrDefaultAsync();
        if (product == null)
        {
            Logger.LogWarning($"Product with id {id} not found.");
        }

        return product;
    }

    public async Task<IEnumerable<GetProductsResponseDto>> GetProductsAsync()
    {
        var products = await Project(_context.Products.AsNoTracking()).ToListAsync();
        if (products.Count == 0)
        {
            Logger.LogWarning("No products found.");
        }

        return products;
    }

    async public Task<GetProductsResponsePaginatorDto> GetProductsPaginatorAsync(int pageNumber, int pageSize, string name)
    {

        var query = _context.Products.AsNoTracking();

        if (name != null && !string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.Name.Contains(name) || (x.Code != null && x.Code.Contains(name)));
        }

        // El total se cuenta después del filtro, para que la paginación coincida con lo buscado.
        var totalRows = await query.CountAsync();

        var products = await Project(query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize))
            .ToListAsync();

        if (products.Count == 0)
        {
            Logger.LogWarning("No products found for the given page.");
            return new GetProductsResponsePaginatorDto(0, new List<GetProductsResponseDto>());
        }

        return new GetProductsResponsePaginatorDto(totalRows, products);
    }

    public async Task<IEnumerable<GetProductsByPatronResponseDto>> GetProductsByPatronAsync(string productNamePatron)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.Name.Contains(productNamePatron) || (p.Code != null && p.Code.Contains(productNamePatron)))
            .OrderBy(p => p.Name)
            .Select(p => new GetProductsByPatronResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                UnitMeasurementId = p.UnitMeasurementId,
                UnitMeasurementDescription = p.UnitMeasurement!.Abbreviation,
                Code = p.Code,
                TracksStock = p.TracksStock,
                Stock = _context.ProductStocks
                    .Where(s => s.ProductId == p.Id && s.Warehouse!.IsActive)
                    .Sum(s => (decimal?)s.Quantity) ?? 0
            })
            .ToListAsync();

        return products;
    }

}
