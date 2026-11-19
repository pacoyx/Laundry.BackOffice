
using Microsoft.EntityFrameworkCore;

public class GetCategoriesProdService : IGetCategoriesProdService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<GetCategoriesProdService> Logger { get; }

    public GetCategoriesProdService(RecepcionDbContext context, IAppLogger<GetCategoriesProdService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public Task<List<GetCategoriesProdResponse>> GetCategoriesProdAsync()
    {
        return _context.CategoryProds
            .Select(x => new GetCategoriesProdResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status
            })
            .ToListAsync();
    }

    public Task<List<GetCategoriesProdShortResponse>> GetCategoriesProdShortAsync()
    {
        return _context.CategoryProds
        .Where(x => x.Status == "A")
        .Select(x => new GetCategoriesProdShortResponse
        {
            Id = x.Id,
            Name = x.Name
        })
        .ToListAsync();
    }

    public Task<CategoryProd> GetCategoryProdByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}