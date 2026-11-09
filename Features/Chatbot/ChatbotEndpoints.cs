public static class ChatbotEndpoints
{
    public static void MapChatbot(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chatbot").WithTags("Chatbot");

        group.MapSendChatbotMessage();
    }
}
