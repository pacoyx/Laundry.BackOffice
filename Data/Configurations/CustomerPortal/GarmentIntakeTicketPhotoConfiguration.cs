using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GarmentIntakeTicketPhotoConfiguration : IEntityTypeConfiguration<GarmentIntakeTicketPhoto>
{
    public void Configure(EntityTypeBuilder<GarmentIntakeTicketPhoto> builder)
    {
        builder.ToTable("GarmentIntakeTicketPhotos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.CreatedByCustomerUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByCustomerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
