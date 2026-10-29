using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ItemPhotoConfiguration : IEntityTypeConfiguration<ItemPhoto>
{
    public void Configure(EntityTypeBuilder<ItemPhoto> builder)
    {
        builder.ToTable("ItemPhotos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.OriginalFileName).HasMaxLength(260);
        builder.Property(x => x.BlobUrl).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Stage).HasDefaultValue(WorkGuideEvidenceStage.Revision);

        builder.HasOne(x => x.WorkGuideMain)
            .WithMany()
            .HasForeignKey(x => x.WorkGuideMainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
