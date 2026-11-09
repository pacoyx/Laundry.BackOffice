public static class GetChatbotStatusEndpoint
{
    public static void MapGetChatbotStatus(this IEndpointRouteBuilder app)
    {
        app.MapGet("/status", (ILlmClient llmClient) =>
        {
            // Refleja el estado real registrado en DI (Chatbot:Enabled + Provider validos),
            // no solo el flag crudo: si el switch esta en true pero el provider no es
            // reconocido, igual se registro DisabledLlmClient y el widget no debe mostrarse.
            var enabled = llmClient is not DisabledLlmClient;

            return Results.Ok(new ApiResponse<ChatbotStatusResponse>
            {
                Success = true,
                Message = "Estado del chatbot.",
                StatusCode = StatusCodes.Status200OK,
                Data = new ChatbotStatusResponse(enabled)
            });
        })
        .WithName("GetChatbotStatus")
        .WithTags("Chatbot")
        .Produces<ApiResponse<ChatbotStatusResponse>>(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}
