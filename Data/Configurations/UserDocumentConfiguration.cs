using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserDocumentConfiguration : IEntityTypeConfiguration<UserDocument>
{
    public void Configure(EntityTypeBuilder<UserDocument> builder)
    {
        builder.ToTable("UserDocuments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.BlobUrl).IsRequired().HasMaxLength(500);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
