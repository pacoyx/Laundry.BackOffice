using Microsoft.EntityFrameworkCore;

// Crea el correlativo de Guia de Remision (TypeProcess "GUIAR") para cada
// sucursal que todavia no tenga uno. Idempotente: no pisa ni duplica
// correlativos ya existentes, solo completa los que falten.
// NOTA: se siembra para TODAS las sucursales (no solo IsActive = true), porque
// IsActive no necesariamente refleja si la sucursal emite facturas.
public class GuiaRemisionNumbersDocumentSeeder : IApplicationSeeder
{
    private const string TypeProcess = "GUIAR"; // max 5 caracteres (NumbersDocument.TypeProcess)
    private const string TypeDoc = "09"; // Codigo SUNAT: Guia de Remision Remitente
    private const string SerieDoc = "T001";

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var branchIds = await context.BranchSales
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var branchIdsConCorrelativo = await context.NumbersDocuments
            .Where(x => x.TypeProcess == TypeProcess)
            .Select(x => x.BranchId)
            .ToListAsync(cancellationToken);

        foreach (var branchId in branchIds.Where(id => !branchIdsConCorrelativo.Contains(id)))
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
