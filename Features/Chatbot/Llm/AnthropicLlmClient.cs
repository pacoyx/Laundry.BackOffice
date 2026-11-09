using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

// Llama directo a la Messages API de Anthropic (sin el SDK oficial, mismo criterio que
// TwilioSmsService/SendGridEmailService: evitar sumar una dependencia nueva solo por esto).
// Activar con Chatbot:Enabled=true, Chatbot:Provider="Anthropic" y Chatbot:ApiKey en configuracion.
public class AnthropicLlmClient : ILlmClient
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    private readonly HttpClient _httpClient;
    private readonly ChatbotSettings _settings;
    private readonly IAppLogger<AnthropicLlmClient> _logger;

    public AnthropicLlmClient(HttpClient httpClient, IOptions<ChatbotSettings> settings, IAppLogger<AnthropicLlmClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<LlmCompletionResult> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<LlmMessage> messages,
        IReadOnlyList<ChatToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        var requestBody = BuildRequestBody(systemPrompt, messages, tools);

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", _settings.ApiKey);
        request.Headers.Add("anthropic-version", AnthropicVersion);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error al llamar a la API de Anthropic: {0} - {1}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"Anthropic respondió {response.StatusCode} al generar la respuesta del chat.");
        }

        return ParseResponse(responseBody);
    }

    private JsonObject BuildRequestBody(string systemPrompt, IReadOnlyList<LlmMessage> messages, IReadOnlyList<ChatToolDefinition> tools)
    {
        var messagesArray = new JsonArray();
        foreach (var message in messages)
        {
            var contentArray = new JsonArray();
            foreach (var block in message.Content)
            {
                contentArray.Add(block switch
                {
                    ChatTextBlock text => new JsonObject { ["type"] = "text", ["text"] = text.Text },
                    ChatToolUseBlock toolUse => new JsonObject
                    {
                        ["type"] = "tool_use",
                        ["id"] = toolUse.Id,
                        ["name"] = toolUse.Name,
                        ["input"] = JsonNode.Parse(toolUse.InputJson)
                    },
                    ChatToolResultBlock toolResult => new JsonObject
                    {
                        ["type"] = "tool_result",
                        ["tool_use_id"] = toolResult.ToolUseId,
                        ["content"] = toolResult.Content
                    },
                    _ => throw new InvalidOperationException($"Tipo de bloque no soportado: {block.GetType().Name}")
                });
            }

            messagesArray.Add(new JsonObject { ["role"] = message.Role, ["content"] = contentArray });
        }

        var toolsArray = new JsonArray();
        foreach (var tool in tools)
        {
            toolsArray.Add(new JsonObject
            {
                ["name"] = tool.Name,
                ["description"] = tool.Description,
                ["input_schema"] = JsonNode.Parse(tool.JsonSchema)
            });
        }

        var body = new JsonObject
        {
            ["model"] = _settings.Model,
            ["max_tokens"] = _settings.MaxTokens,
            ["system"] = systemPrompt,
            ["messages"] = messagesArray
        };

        if (toolsArray.Count > 0)
        {
            body["tools"] = toolsArray;
        }

        return body;
    }

    private static LlmCompletionResult ParseResponse(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        var stopReason = root.TryGetProperty("stop_reason", out var stopReasonEl) ? stopReasonEl.GetString() ?? "end_turn" : "end_turn";

        var blocks = new List<ChatContentBlock>();
        foreach (var contentEl in root.GetProperty("content").EnumerateArray())
        {
            var type = contentEl.GetProperty("type").GetString();
            switch (type)
            {
                case "text":
                    blocks.Add(new ChatTextBlock(contentEl.GetProperty("text").GetString() ?? string.Empty));
                    break;
                case "tool_use":
                    blocks.Add(new ChatToolUseBlock(
                        contentEl.GetProperty("id").GetString() ?? string.Empty,
                        contentEl.GetProperty("name").GetString() ?? string.Empty,
                        contentEl.GetProperty("input").GetRawText()));
                    break;
            }
        }

        return new LlmCompletionResult(blocks, stopReason);
    }
}
