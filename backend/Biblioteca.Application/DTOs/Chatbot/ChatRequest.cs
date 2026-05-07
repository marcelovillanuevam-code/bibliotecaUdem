using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Chatbot;

public sealed record ChatRequest([Required, MaxLength(500)] string Message);
