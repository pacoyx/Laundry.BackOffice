using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ElectronicInvoiceSubmissionConfiguration : IEntityTypeConfiguration<ElectronicInvoiceSubmission>
{
    public void Configure(EntityTypeBuilder<ElectronicInvoiceSubmission> builder)
    {
        builder.ToTable("ElectronicInvoiceSubmissions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SunatDescription).HasMaxLength(500);
        builder.Property(x => x.SunatResponseCode).HasMaxLength(50);
        builder.Property(x => x.DocumentUrl).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.RawResponseJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.InvoiceId);

        builder.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
