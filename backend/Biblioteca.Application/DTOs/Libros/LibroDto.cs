namespace Biblioteca.Application.DTOs.Libros;

public sealed record LibroDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string? Isbn,
    string? Publisher,
    short? PublicationYear,
    string? Edition,
    string Language,
    DateTime CreatedAt,
    DateTime UpdatedAt);
