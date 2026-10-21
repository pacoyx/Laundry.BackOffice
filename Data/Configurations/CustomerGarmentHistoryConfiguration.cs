using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CustomerGarmentHistoryConfiguration : IEntityTypeConfiguration<CustomerGarmentHistory>
{
    public void Configure(EntityTypeBuilder<CustomerGarmentHistory> builder)
    {
        builder.ToTable("CustomerGarmentHistories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Detail).HasMaxLength(500);
        builder.Property(x => x.NormalizedDetail).HasMaxLength(500);
        builder.Property(x => x.Status).HasMaxLength(1).HasDefaultValue("A");

        // Evita duplicados por (cliente, tipo de prenda, detalle normalizado) mientras el registro este activo.
        builder.HasIndex(x => new { x.CustomerId, x.ProductId, x.NormalizedDetail })
            .IsUnique()
            .HasFilter("[Status] = 'A'")
            .HasDatabaseName("IX_CustomerGarmentHistories_Customer_Product_Detail_ActiveOnly");

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
