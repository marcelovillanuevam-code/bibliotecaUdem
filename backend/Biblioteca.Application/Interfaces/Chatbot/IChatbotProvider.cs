using Biblioteca.Application.DTOs.Chatbot;

namespace Biblioteca.Application.Interfaces.Chatbot;

// Interfaz de chatbot; permite swapear a un proveedor LLM real sin cambiar el controlador.
public interface IChatbotProvider
{
    Task<ChatResponse> AskAsync(string message, Guid userId, CancellationToken ct);
}
