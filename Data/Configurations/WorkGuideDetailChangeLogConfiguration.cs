using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WorkGuideDetailChangeLogConfiguration : IEntityTypeConfiguration<WorkGuideDetailChangeLog>
{
    public void Configure(EntityTypeBuilder<WorkGuideDetailChangeLog> builder)
    {
        builder.ToTable("WorkGuideDetailChangeLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Observation).HasMaxLength(500);

        builder.HasIndex(x => x.WorkGuideMainId);

        builder.HasOne(x => x.WorkGuideMain)
            .WithMany()
            .HasForeignKey(x => x.WorkGuideMainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WorkGuideDetail)
            .WithMany()
            .HasForeignKey(x => x.WorkGuideDetailId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServiceItem)
            .WithMany()
            .HasForeignKey(x => x.ServiceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
