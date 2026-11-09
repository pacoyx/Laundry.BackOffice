using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyWorkerConfiguration : IEntityTypeConfiguration<CompanyWorker>
{
    public void Configure(EntityTypeBuilder<CompanyWorker> builder)
    {
        builder.ToTable("CompanyWorkers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Area).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => new { x.CustomerId, x.Code }).IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
