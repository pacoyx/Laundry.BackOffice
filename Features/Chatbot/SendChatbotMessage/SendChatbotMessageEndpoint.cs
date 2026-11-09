using FluentValidation;
using Microsoft.EntityFrameworkCore;

public static class SendChatbotMessageEndpoint
{
    private const int MaxToolRoundTrips = 4;

    public static void MapSendChatbotMessage(this IEndpointRouteBuilder app)
    {
        app.MapPost("/messages", async (
            ChatbotMessageRequest request,
            IValidator<ChatbotMessageRequest> validator,
            RecepcionDbContext db,
            ILlmClient llmClient,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }

            var conversation = await db.ChatConversations
                .Include(x => x.Messages)
                .Where(x => x.Channel == request.Channel && x.ExternalReference == request.ExternalReference)
                .OrderByDescending(x => x.LastMessageAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (conversation is null)
            {
                conversation = new ChatConversation
                {
                    Channel = request.Channel,
                    ExternalReference = request.ExternalReference,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };
                await db.ChatConversations.AddAsync(conversation, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // El historial persistido solo guarda texto plano alternando user/assistant: el
            // vaiven de tool_use/tool_result de este turno vive unicamente en memoria durante
            // este request y no se persiste, asi evitamos tener que reproducir ids de tool_use
            // de turnos anteriores en requests futuros.
            var history = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => m.Role == ChatMessageRole.User
                    ? LlmMessage.User(m.Content)
                    : new LlmMessage("assistant", new List<ChatContentBlock> { new ChatTextBlock(m.Content) }))
                .ToList();

            var workingMessages = new List<LlmMessage>(history) { LlmMessage.User(request.Message) };

            var finalReply = string.Empty;
            for (var i = 0; i < MaxToolRoundTrips; i++)
            {
                var completion = await llmClient.CompleteAsync(ChatbotSystemPrompt.Text, workingMessages, ChatbotToolDefinitions.All, cancellationToken);
                finalReply = completion.TextOrEmpty;

                var toolUses = completion.ToolUses.ToList();
                if (toolUses.Count == 0)
                {
                    break;
                }

                workingMessages.Add(new LlmMessage("assistant", completion.Content));

                var toolResults = new List<ChatContentBlock>();
                foreach (var toolUse in toolUses)
                {
                    var result = await ChatbotToolExecutor.ExecuteAsync(toolUse.Name, toolUse.InputJson, db, cancellationToken);
                    toolResults.Add(new ChatToolResultBlock(toolUse.Id, result));
                }

                workingMessages.Add(new LlmMessage("user", toolResults));
            }

            if (string.IsNullOrWhiteSpace(finalReply))
            {
                finalReply = "No pude procesar tu consulta en este momento. Intenta de nuevo o escríbenos por WhatsApp.";
            }

            var now = DateTime.UtcNow;
            conversation.Messages.Add(new ChatMessage { Role = ChatMessageRole.User, Content = request.Message, CreatedAt = now });
            conversation.Messages.Add(new ChatMessage { Role = ChatMessageRole.Assistant, Content = finalReply, CreatedAt = now });
            conversation.LastMessageAt = now;

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new ApiResponse<ChatbotMessageResponse>
            {
                Success = true,
                Message = "Respuesta generada.",
                StatusCode = StatusCodes.Status200OK,
                Data = new ChatbotMessageResponse(conversation.Id, finalReply)
            });
        })
        .WithName("SendChatbotMessage")
        .WithTags("Chatbot")
        .Produces<ApiResponse<ChatbotMessageResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireRateLimiting("chatbot-messages")
        .AllowAnonymous();
    }
}
