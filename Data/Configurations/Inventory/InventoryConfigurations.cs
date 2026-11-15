using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique();
        // Un solo almacén por defecto por sucursal.
        builder.HasIndex(x => x.BranchSalesId).IsUnique().HasFilter("[IsDefault] = 1");

        builder.HasOne(x => x.BranchSales)
            .WithMany()
            .HasForeignKey(x => x.BranchSalesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
    public void Configure(EntityTypeBuilder<ProductStock> builder)
    {
        builder.ToTable("ProductStocks");
        builder.HasKey(x => new { x.ProductId, x.WarehouseId });

        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.AverageCost).HasPrecision(18, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.WarehouseId);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(300);

        builder.HasIndex(x => x.MovementDate);
        builder.HasIndex(x => new { x.WarehouseId, x.MovementDate });
        builder.HasIndex(x => x.PurchaseInvoiceId);
        builder.HasIndex(x => x.SaleId);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetWarehouse)
            .WithMany()
            .HasForeignKey(x => x.TargetWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Area)
            .WithMany()
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PurchaseInvoice)
            .WithMany()
            .HasForeignKey(x => x.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.InventoryMovement)
            .HasForeignKey(x => x.InventoryMovementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InventoryMovementLineConfiguration : IEntityTypeConfiguration<InventoryMovementLine>
{
    public void Configure(EntityTypeBuilder<InventoryMovementLine> builder)
    {
        builder.ToTable("InventoryMovementLines");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.TotalCost).HasPrecision(18, 4);
        builder.Property(x => x.BalanceQuantity).HasPrecision(18, 4);
        builder.Property(x => x.BalanceAverageCost).HasPrecision(18, 4);

        // Kardex: líneas de un producto en un almacén, en orden.
        builder.HasIndex(x => new { x.ProductId, x.WarehouseId, x.Id });

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
