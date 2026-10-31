using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FixedAssetMaintenanceConfiguration : IEntityTypeConfiguration<FixedAssetMaintenance>
{
    public void Configure(EntityTypeBuilder<FixedAssetMaintenance> builder)
    {
        builder.ToTable("FixedAssetMaintenances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Observations).HasMaxLength(500);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.FixedAssetMaintenance)
            .HasForeignKey(x => x.FixedAssetMaintenanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
