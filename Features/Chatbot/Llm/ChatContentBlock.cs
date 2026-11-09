// Representacion generica (no atada a un proveedor especifico) de los bloques que puede
// traer un mensaje de ida o vuelta con el LLM. Cada implementacion de ILlmClient traduce
// estos tipos hacia/desde el formato propio de su API (Anthropic, OpenAI, etc.).
public abstract record ChatContentBlock;

public sealed record ChatTextBlock(string Text) : ChatContentBlock;

public sealed record ChatToolUseBlock(string Id, string Name, string InputJson) : ChatContentBlock;

public sealed record ChatToolResultBlock(string ToolUseId, string Content) : ChatContentBlock;

public sealed record LlmMessage(string Role, List<ChatContentBlock> Content)
{
    public static LlmMessage User(string text) => new("user", new List<ChatContentBlock> { new ChatTextBlock(text) });

    public static LlmMessage ToolResult(string toolUseId, string content) =>
        new("user", new List<ChatContentBlock> { new ChatToolResultBlock(toolUseId, content) });
}

public sealed record ChatToolDefinition(string Name, string Description, string JsonSchema);

public sealed record LlmCompletionResult(List<ChatContentBlock> Content, string StopReason)
{
    public IEnumerable<ChatToolUseBlock> ToolUses => Content.OfType<ChatToolUseBlock>();

    public string TextOrEmpty => string.Concat(Content.OfType<ChatTextBlock>().Select(b => b.Text));
}
