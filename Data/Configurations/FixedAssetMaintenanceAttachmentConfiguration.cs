using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FixedAssetMaintenanceAttachmentConfiguration : IEntityTypeConfiguration<FixedAssetMaintenanceAttachment>
{
    public void Configure(EntityTypeBuilder<FixedAssetMaintenanceAttachment> builder)
    {
        builder.ToTable("FixedAssetMaintenanceAttachments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
    }
}
