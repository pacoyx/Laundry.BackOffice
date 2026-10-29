using Microsoft.EntityFrameworkCore;

public class ZoneSeeder : IApplicationSeeder
{
    private static readonly string[] Names = { "Local", "Sur", "Norte", "Centro" };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.Zones
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var missingNames = Names.Except(existingNames);

        foreach (var name in missingNames)
        {
            context.Zones.Add(new Zone { Name = name, IsActive = true });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
