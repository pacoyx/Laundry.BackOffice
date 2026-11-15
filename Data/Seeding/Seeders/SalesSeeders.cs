using Microsoft.EntityFrameworkCore;

/// <summary>Correlativo de notas de venta por sucursal (serie NV01). Idempotente.</summary>
public class SalesNumbersDocumentSeeder : IApplicationSeeder
{
    public const string TypeProcess = "VENTA"; // max 5 caracteres (NumbersDocument.TypeProcess)
    private const string TypeDoc = "NV";       // Nota de venta interna (no es comprobante SUNAT)
    private const string SerieDoc = "NV01";

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var branchIds = await context.BranchSales.Select(x => x.Id).ToListAsync(cancellationToken);
        var withSeries = await context.NumbersDocuments
            .Where(x => x.TypeProcess == TypeProcess)
            .Select(x => x.BranchId)
            .ToListAsync(cancellationToken);

        foreach (var branchId in branchIds.Where(id => !withSeries.Contains(id)))
        {
            context.NumbersDocuments.Add(new NumbersDocument
            {
                BranchId = branchId,
                TypeDoc = TypeDoc,
                SerieDoc = SerieDoc,
                NumberDoc = 1,
                Status = "A",
                TypeProcess = TypeProcess
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Cliente genérico para ventas sin datos del comprador. La caja requiere un cliente en cada movimiento.
/// Se identifica por InternalCode = "VARIOS". Idempotente.
/// </summary>
public class WalkInCustomerSeeder : IApplicationSeeder
{
    public const string InternalCode = "VARIOS";

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Customers.AnyAsync(x => x.InternalCode == InternalCode, cancellationToken))
        {
            return;
        }

        context.Customers.Add(new Customer
        {
            FirtsName = "CLIENTES VARIOS",
            LastName = string.Empty,
            InternalCode = InternalCode,
            Status = "A",
            IsActive = true,
            CustomerType = CustomerType.Individual,
            Origin = CustomerOrigin.Recepcion
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
