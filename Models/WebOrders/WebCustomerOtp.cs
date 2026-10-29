public class WebCustomerOtp
{
    public long Id { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int AttemptCount { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }
}
