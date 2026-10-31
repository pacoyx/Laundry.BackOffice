using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GuiaRemisionConfiguration : IEntityTypeConfiguration<GuiaRemision>
{
    public void Configure(EntityTypeBuilder<GuiaRemision> builder)
    {
        builder.ToTable("GuiasRemision");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Serie).IsRequired().HasMaxLength(5);
        builder.Property(x => x.Numero).IsRequired().HasMaxLength(10);
        builder.Property(x => x.MotivoTraslado).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DriverName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.DriverDni).IsRequired().HasMaxLength(20);
        builder.Property(x => x.OriginAddress).IsRequired().HasMaxLength(300);
        builder.Property(x => x.DeliveryAddress).IsRequired().HasMaxLength(300);

        builder.HasIndex(x => x.InvoiceId).IsUnique();

        builder.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
