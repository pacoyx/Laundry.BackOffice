public record ChatbotMessageRequest(ChatChannel Channel, string ExternalReference, string Message);

public record ChatbotMessageResponse(long ConversationId, string Reply);
