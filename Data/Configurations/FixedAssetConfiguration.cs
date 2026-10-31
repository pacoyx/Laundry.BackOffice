using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FixedAssetConfiguration : IEntityTypeConfiguration<FixedAsset>
{
    public void Configure(EntityTypeBuilder<FixedAsset> builder)
    {
        builder.ToTable("FixedAssets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisposalReason).HasMaxLength(300);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PurchaseInvoice)
            .WithMany()
            .HasForeignKey(x => x.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResponsibleUser)
            .WithMany()
            .HasForeignKey(x => x.ResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Maintenances)
            .WithOne(x => x.FixedAsset)
            .HasForeignKey(x => x.FixedAssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reassignments)
            .WithOne(x => x.FixedAsset)
            .HasForeignKey(x => x.FixedAssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
