using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WebOrderConfiguration : IEntityTypeConfiguration<WebOrder>
{
    public void Configure(EntityTypeBuilder<WebOrder> builder)
    {
        builder.ToTable("WebOrders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(40);
        builder.Property(x => x.DeliveryAddress).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ContactPhone).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Status).HasDefaultValue(WebOrderStatus.Pendiente);

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.Status });
        builder.HasIndex(x => new { x.EmployeeId, x.Status });

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.District)
            .WithMany()
            .HasForeignKey(x => x.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ZonePriceList)
            .WithMany()
            .HasForeignKey(x => x.ZonePriceListId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WorkGuideMain)
            .WithMany()
            .HasForeignKey(x => x.WorkGuideMainId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ConfirmedByUser)
            .WithMany()
            .HasForeignKey(x => x.ConfirmedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Details)
            .WithOne(x => x.WebOrder)
            .HasForeignKey(x => x.WebOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
