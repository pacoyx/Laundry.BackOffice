// Implementacion temporal mientras Chatbot:Enabled=false o no haya un proveedor configurado.
// Responde con un mensaje de fallback en vez de romper, igual que LoggingSmsService/LoggingEmailService.
public class DisabledLlmClient : ILlmClient
{
    public Task<LlmCompletionResult> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<ChatToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        var fallback = new ChatTextBlock(
            "El chat automático no está disponible en este momento. Escríbenos por WhatsApp y te ayudamos enseguida.");

        return Task.FromResult(new LlmCompletionResult(new List<ChatContentBlock> { fallback }, "end_turn"));
    }
}
