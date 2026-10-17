using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PickupGuideRevisionPhotoConfiguration : IEntityTypeConfiguration<PickupGuideRevisionPhoto>
{
    public void Configure(EntityTypeBuilder<PickupGuideRevisionPhoto> builder)
    {
        builder.ToTable("PickupGuideRevisionPhotos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.OriginalFileName).HasMaxLength(260);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.PickupGuideRevision)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.PickupGuideRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
