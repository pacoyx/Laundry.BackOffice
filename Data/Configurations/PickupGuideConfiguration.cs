using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PickupGuideConfiguration : IEntityTypeConfiguration<PickupGuide>
{
    public void Configure(EntityTypeBuilder<PickupGuide> builder)
    {
        builder.ToTable("PickupGuides");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GuideNumber).IsRequired().HasMaxLength(40);
        builder.Property(x => x.PhysicalGuideNumber).HasMaxLength(50);
        builder.Property(x => x.Observation).HasMaxLength(500);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasDefaultValue(PickupGuideStatus.Draft);
        builder.Property(x => x.EmailDeliveryStatus).HasDefaultValue(PickupGuideEmailStatus.Pending);
        builder.Property(x => x.LastEmailError).HasMaxLength(1000);

        builder.HasIndex(x => x.GuideNumber).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.Status });

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BranchSales)
            .WithMany()
            .HasForeignKey(x => x.BranchSalesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CustomerPriceList)
            .WithMany()
            .HasForeignKey(x => x.CustomerPriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.PickupGuide)
            .HasForeignKey(x => x.PickupGuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.PickupGuide)
            .HasForeignKey(x => x.PickupGuideId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Revisions)
            .WithOne(x => x.PickupGuide)
            .HasForeignKey(x => x.PickupGuideId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
