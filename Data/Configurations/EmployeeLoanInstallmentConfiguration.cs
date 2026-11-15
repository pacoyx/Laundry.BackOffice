using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmployeeLoanInstallmentConfiguration : IEntityTypeConfiguration<EmployeeLoanInstallment>
{
    public void Configure(EntityTypeBuilder<EmployeeLoanInstallment> builder)
    {
        builder.ToTable("EmployeeLoanInstallments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.EmployeeLoanId, x.InstallmentNumber }).IsUnique();
        builder.HasIndex(x => x.PayrollId);

        builder.HasOne(x => x.EmployeeLoan)
            .WithMany(x => x.Installments)
            .HasForeignKey(x => x.EmployeeLoanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Payroll)
            .WithMany()
            .HasForeignKey(x => x.PayrollId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
