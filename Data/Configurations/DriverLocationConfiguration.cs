using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DriverLocationConfiguration : IEntityTypeConfiguration<DriverLocation>
{
    public void Configure(EntityTypeBuilder<DriverLocation> builder)
    {
        builder.ToTable("DriverLocations");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Para traer "la ultima posicion de cada chofer" ordenando por Id descendente
        // agrupado por EmployeeId (mismo patron que LocationWorkGuide).
        builder.HasIndex(x => new { x.EmployeeId, x.Id });

        // Soporta el DELETE por antiguedad del job de limpieza (ver DriverLocationCleanupService)
        // sin tener que escanear toda la tabla en cada corrida.
        builder.HasIndex(x => x.CreatedAt);
    }
}
