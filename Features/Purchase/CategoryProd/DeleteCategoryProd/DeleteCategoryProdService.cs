using Microsoft.EntityFrameworkCore;

public class DeleteCategoryProdService : IDeleteCategoryProdService
{
    private readonly RecepcionDbContext _context;
    public IAppLogger<DeleteCategoryProdService> Logger { get; }

    public DeleteCategoryProdService(RecepcionDbContext context, IAppLogger<DeleteCategoryProdService> logger)
    {
        _context = context;
        Logger = logger;
    }

    public async Task<string> DeleteCategoryProdAsync(int id)
    {
        var categoryProd = await _context.CategoryProds.FirstOrDefaultAsync(x => x.Id == id);
        if (categoryProd == null)
        {
            return $"Categoría con ID {id} no encontrada.";
        }

        // Baja lógica: Product tiene FK obligatoria a CategoryProdId, un delete físico
        // rompería la integridad referencial (o arrastraría productos en cascada).
        categoryProd.Status = "I";

        await _context.SaveChangesAsync();

        return "OK";
    }
}
