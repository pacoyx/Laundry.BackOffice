public class ChatMessage
{
    public long Id { get; set; }

    public long ConversationId { get; set; }

    public ChatConversation Conversation { get; set; } = null!;

    public ChatMessageRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
