using Microsoft.EntityFrameworkCore;

public class BankAccountTypeSeeder : IApplicationSeeder
{
    private static readonly string[] Names =
    {
        "Cuenta Corriente", "Cuenta Ahorros", "Cuenta Detracción", "Cuenta CTS", "Cuenta Recaudadora"
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.BankAccountTypes
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var missingNames = Names.Except(existingNames);

        foreach (var name in missingNames)
        {
            context.BankAccountTypes.Add(new BankAccountType { Name = name, IsActive = true, CreatedAt = DateTime.UtcNow });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
