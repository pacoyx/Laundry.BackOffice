public class ChatbotSettings
{
    // Switch maestro: aunque haya Provider/ApiKey configurados, si esto es false el bot
    // queda apagado (DisabledLlmClient se registra en su lugar).
    public bool Enabled { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-haiku-4-5-20251001";

    public int MaxTokens { get; set; } = 1024;
}
