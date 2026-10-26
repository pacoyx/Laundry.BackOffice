using Microsoft.EntityFrameworkCore;

public class RoleSeeder : IApplicationSeeder
{
    private static readonly (string Name, string Description)[] Roles =
    {
        (RoleNames.Administrador, "Acceso total al sistema"),
        (RoleNames.Recepcion, "Personal de recepcion"),
        (RoleNames.RecepcionBasico, "Personal de recepcion con acceso solo a Emision y Retiro"),
        (RoleNames.Compras, "Personal de compras y proveedores"),
        (RoleNames.Rrhh, "Personal de Recursos Humanos"),
        (RoleNames.Gerencia, "Gerencia con vision transversal del negocio"),
        (RoleNames.OperacionesCorporativas, "Personal de Operaciones Corporativas"),
        (RoleNames.Recolector, "Personal de recojo de ropa en campo (PWA Pickup Mobile)")
    };

    public async Task SeedAsync(RecepcionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.Roles
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var missing = Roles.Where(r => !existingNames.Contains(r.Name));

        foreach (var role in missing)
        {
            context.Roles.Add(new Role
            {
                Name = role.Name,
                Description = role.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
