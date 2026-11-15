using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number).IsRequired().HasMaxLength(30);
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PaymentType).IsRequired().HasMaxLength(5);
        builder.Property(x => x.PaymentDescription).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(300);
        builder.Property(x => x.CancelReason).HasMaxLength(300);

        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.Igv).HasPrecision(18, 2);
        builder.Property(x => x.AmountReceived).HasPrecision(18, 2);
        builder.Property(x => x.Change).HasPrecision(18, 2);

        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.SaleDate);
        builder.HasIndex(x => x.CashBoxMainId);

        builder.HasOne(x => x.BranchSales).WithMany().HasForeignKey(x => x.BranchSalesId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.Sale)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);

        builder.HasIndex(x => x.ProductId);

        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
