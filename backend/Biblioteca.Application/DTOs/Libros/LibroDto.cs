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
    IReadOnlyCollection<LibroAutorDto> Authors,
    IReadOnlyCollection<LibroMateriaDto> Subjects,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalCopies,
    int AvailableCopies);
