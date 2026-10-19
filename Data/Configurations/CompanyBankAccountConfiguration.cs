using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CompanyBankAccountConfiguration : IEntityTypeConfiguration<CompanyBankAccount>
{
    public void Configure(EntityTypeBuilder<CompanyBankAccount> builder)
    {
        builder.ToTable("CompanyBankAccounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CurrencyId).IsRequired().HasMaxLength(3);
        builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(30);
        builder.Property(x => x.InterbankAccountNumber).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankAccountType)
            .WithMany()
            .HasForeignKey(x => x.BankAccountTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
