using Microsoft.EntityFrameworkCore;

// Precarga las categorias de Activo Fijo con las tasas maximas de depreciacion
// que reconoce SUNAT (Reglamento de la Ley del Impuesto a la Renta, Art. 22) como
// base referencial para el calculo de linea recta. No reemplaza un libro contable.
public class FixedAssetCategorySeeder : IApplicationSeeder
{
    private static readonly (string Name, decimal RatePercent, int UsefulLifeYears)[] Categories =
    {
        ("Maquinaria y Equipo", 10m, 10),
        ("Equipos de Computo", 25m, 4),
        ("Muebles y Enseres", 10m, 10),
        ("Herramientas", 10m, 10),
        ("Otros", 10m, 10)
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.FixedAssetCategories
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        foreach (var category in Categories.Where(c => !existingNames.Contains(c.Name)))
        {
            context.FixedAssetCategories.Add(new FixedAssetCategory
            {
                Name = category.Name,
                DefaultDepreciationRatePercent = category.RatePercent,
                DefaultUsefulLifeYears = category.UsefulLifeYears
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
