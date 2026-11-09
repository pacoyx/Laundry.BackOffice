using System.ComponentModel.DataAnnotations;

public class ChatConversation
{
    public long Id { get; set; }

    public ChatChannel Channel { get; set; }

    // Para Web: un GUID anonimo generado por el navegador (localStorage). Para WhatsApp: el
    // numero de telefono. Es lo que permite reconstruir la conversacion sin depender de un login.
    [MaxLength(200)]
    public string ExternalReference { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime LastMessageAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
