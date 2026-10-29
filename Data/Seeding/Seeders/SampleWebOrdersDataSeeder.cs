using Microsoft.EntityFrameworkCore;

// Datos de EJEMPLO para poder probar el flujo de pedidos web de punta a punta
// (distritos, tarifas por zona, sucursal central y correlativo). Pensado para
// quitarse del DI o reemplazarse por carga real de datos antes de produccion.
public class SampleWebOrdersDataSeeder : IApplicationSeeder
{
    private static readonly Dictionary<string, string[]> DistrictsByZone = new()
    {
        ["Local"] = new[] { "Miraflores", "San Isidro", "Barranco" },
        ["Sur"] = new[] { "Chorrillos", "Surco", "San Juan de Miraflores" },
        ["Norte"] = new[] { "Los Olivos", "San Martin de Porres", "Independencia" },
        ["Centro"] = new[] { "Lima Cercado", "Breña", "La Victoria" }
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedDistrictsAsync(context, cancellationToken);
        await SeedZonePriceListsAsync(context, cancellationToken);
        await SeedCentralBranchAsync(context, cancellationToken);
        await SeedNumbersDocumentAsync(context, cancellationToken);
    }

    private static async Task SeedDistrictsAsync(RecepcionDbContext context, CancellationToken cancellationToken)
    {
        var existingNames = await context.Districts.Select(x => x.Name).ToListAsync(cancellationToken);
        var zones = await context.Zones.ToListAsync(cancellationToken);

        foreach (var (zoneName, districtNames) in DistrictsByZone)
        {
            var zone = zones.FirstOrDefault(x => x.Name == zoneName);
            if (zone is null)
            {
                continue;
            }

            foreach (var name in districtNames.Where(n => !existingNames.Contains(n)))
            {
                context.Districts.Add(new District { Name = name, ZoneId = zone.Id, IsActive = true });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedZonePriceListsAsync(RecepcionDbContext context, CancellationToken cancellationToken)
    {
        var zonesWithoutActiveList = await context.Zones
            .Where(z => !context.ZonePriceLists.Any(p => p.ZoneId == z.Id && p.IsActive))
            .ToListAsync(cancellationToken);

        if (zonesWithoutActiveList.Count == 0)
        {
            return;
        }

        var services = await context.ProdServices.Where(x => x.Status == "A").ToListAsync(cancellationToken);
        if (services.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        foreach (var zone in zonesWithoutActiveList)
        {
            context.ZonePriceLists.Add(new ZonePriceList
            {
                ZoneId = zone.Id,
                Name = $"Tarifa de ejemplo - {zone.Name}",
                Version = 1,
                EffectiveFrom = now,
                IsActive = true,
                CreatedAt = now,
                Details = services.Select(s => new ZonePriceListDetail
                {
                    ServiceItemId = s.Id,
                    UnitPrice = s.Price
                }).ToList()
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCentralBranchAsync(RecepcionDbContext context, CancellationToken cancellationToken)
    {
        var hasCentral = await context.BranchSales.AnyAsync(x => x.IsCentral, cancellationToken);
        if (hasCentral)
        {
            return;
        }

        var firstBranch = await context.BranchSales.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (firstBranch is null)
        {
            return;
        }

        firstBranch.IsCentral = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedNumbersDocumentAsync(RecepcionDbContext context, CancellationToken cancellationToken)
    {
        var centralBranch = await context.BranchSales.FirstOrDefaultAsync(x => x.IsCentral, cancellationToken);
        if (centralBranch is null)
        {
            return;
        }

        var exists = await context.NumbersDocuments
            .AnyAsync(x => x.BranchId == centralBranch.Id && x.TypeProcess == "WEB", cancellationToken);
        if (exists)
        {
            return;
        }

        context.NumbersDocuments.Add(new NumbersDocument
        {
            BranchId = centralBranch.Id,
            TypeDoc = "GS",
            SerieDoc = "0002",
            NumberDoc = 1,
            Status = "A",
            TypeProcess = "WEB"
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
