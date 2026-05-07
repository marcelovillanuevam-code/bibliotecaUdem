namespace Biblioteca.Application.DTOs.Chatbot;

public sealed record ChatResponse(string Reply, IReadOnlyCollection<ChatAction>? Actions);
