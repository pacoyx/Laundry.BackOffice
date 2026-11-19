using Microsoft.EntityFrameworkCore;

public class UpdateCategoryProdService : IUpdateCategoryProdService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<UpdateCategoryProdService> Logger { get; }

    public UpdateCategoryProdService(RecepcionDbContext context, IAppLogger<UpdateCategoryProdService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<string> UpdateCategoryProdAsync(UpdateCategoryProdRequestDto request)
    {
        var categoryProd = await _context.CategoryProds.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (categoryProd == null)
        {
            return $"Categoría con ID {request.Id} no encontrada.";
        }

        categoryProd.Name = request.Name;
        categoryProd.Description = request.Description;
        categoryProd.Status = request.Status;

        await _context.SaveChangesAsync();

        return "OK";
    }
}
