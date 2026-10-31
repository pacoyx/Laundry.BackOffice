
public class SharedDocumentRecipient
{
    public long Id { get; set; }

    public long SharedDocumentId { get; set; }

    public int UserId { get; set; }

    public SharedDocument SharedDocument { get; set; } = null!;

    public User User { get; set; } = null!;
}
