using System.ComponentModel.DataAnnotations;

public class CompanyBankAccount
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public int BankId { get; set; }

    public int BankAccountTypeId { get; set; }

    [MaxLength(3)]
    public string CurrencyId { get; set; } = "PEN";

    [MaxLength(30)]
    public string AccountNumber { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? InterbankAccountNumber { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Company? Company { get; set; }

    public Bank? Bank { get; set; }

    public BankAccountType? BankAccountType { get; set; }
}
