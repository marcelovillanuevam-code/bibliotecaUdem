namespace Biblioteca.Application.DTOs.Libros;

public sealed record LibroFichaDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string? Isbn,
    string? Publisher,
    short? PublicationYear,
    string? Edition,
    string Language,
    string? SummaryJson,
    string? MetadataJson,
    IReadOnlyCollection<LibroAutorDto> Authors,
    IReadOnlyCollection<LibroMateriaDto> Subjects,
    DateTime CreatedAt,
    DateTime UpdatedAt);
