using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CustomerBillingProfile
{
    public int CustomerId { get; set; }

    [MaxLength(20)]
    public string TaxId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CommercialName { get; set; }

    [MaxLength(300)]
    public string FiscalAddress { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? BillingEmail { get; set; }

    public int PaymentTermDays { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditLimit { get; set; }

    public bool IsCreditEnabled { get; set; }

    public bool RemindersEnabled { get; set; } = true;

    [MaxLength(300)]
    public string? RemindersSuppressedReason { get; set; }

    public Customer Customer { get; set; } = null!;
}
