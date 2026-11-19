
public class CreateCategoryProdService : ICreateCategoryProdService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<CreateCategoryProdService> Logger { get; }

    public CreateCategoryProdService(RecepcionDbContext context, IAppLogger<CreateCategoryProdService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public Task<CreateCategoryProdResponseDto> CreateCategoryProdAsync(CreateCategoryProdDto categoryProd)
    {
        var newCategoryProd = new CategoryProd
        {
            Name = categoryProd.Name,
            Description = categoryProd.Description,
            Status = categoryProd.Status
        };

        _context.CategoryProds.Add(newCategoryProd);
        _context.SaveChanges();

        var response = new CreateCategoryProdResponseDto
        {
            Id = newCategoryProd.Id,
            Name = newCategoryProd.Name,
            Description = newCategoryProd.Description,
            Status = newCategoryProd.Status
        };
        
        return Task.FromResult(response);
    }
}