public class WebCustomerAccount
{
    public long Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Phone { get; set; } = string.Empty;

    public bool IsPhoneVerified { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }
}
