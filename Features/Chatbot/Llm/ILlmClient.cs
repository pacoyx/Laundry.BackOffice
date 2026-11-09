public interface ILlmClient
{
    Task<LlmCompletionResult> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<ChatToolDefinition> tools,
        CancellationToken cancellationToken = default);
}
