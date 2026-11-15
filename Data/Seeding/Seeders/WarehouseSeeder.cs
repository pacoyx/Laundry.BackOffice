using Microsoft.EntityFrameworkCore;

/// <summary>
/// Garantiza que cada sucursal activa tenga su almacén por defecto ("Almacén principal").
/// Es idempotente: solo crea el almacén de las sucursales que aún no lo tienen.
/// </summary>
public class WarehouseSeeder : IApplicationSeeder
{
    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var branchesWithDefault = await context.Warehouses
            .Where(x => x.IsDefault)
            .Select(x => x.BranchSalesId)
            .ToListAsync(cancellationToken);

        var branches = await context.BranchSales
            .Where(x => x.IsActive && !branchesWithDefault.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (branches.Count == 0)
        {
            return;
        }

        var existingCodes = await context.Warehouses.Select(x => x.Code).ToListAsync(cancellationToken);

        foreach (var branch in branches)
        {
            var code = $"ALM-{branch.Id:D3}";
            if (existingCodes.Contains(code))
            {
                code = $"ALM-{branch.Id:D3}-{Guid.NewGuid():N}"[..20];
            }

            context.Warehouses.Add(new Warehouse
            {
                BranchSalesId = branch.Id,
                Code = code,
                Name = $"Almacén principal - {branch.Description}".Length > 100
                    ? "Almacén principal"
                    : $"Almacén principal - {branch.Description}",
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
