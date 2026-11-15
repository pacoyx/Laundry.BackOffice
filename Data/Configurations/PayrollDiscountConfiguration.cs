using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PayrollDiscountConfiguration : IEntityTypeConfiguration<PayrollDiscount>
{
    public void Configure(EntityTypeBuilder<PayrollDiscount> builder)
    {
        builder.ToTable("PayrollDiscounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.PayrollPeriodId, x.EmployeeId });

        builder.HasOne(x => x.PayrollPeriod)
            .WithMany()
            .HasForeignKey(x => x.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
