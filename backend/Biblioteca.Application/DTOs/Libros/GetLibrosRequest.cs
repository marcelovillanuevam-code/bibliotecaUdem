namespace Biblioteca.Application.DTOs.Libros;

public sealed class GetLibrosRequest
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public string? Subject { get; init; }
    public string? Isbn { get; init; }
    public string? Publisher { get; init; }
    public string? Language { get; init; }
}
