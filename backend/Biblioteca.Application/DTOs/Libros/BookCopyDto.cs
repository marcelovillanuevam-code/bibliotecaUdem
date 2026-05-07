namespace Biblioteca.Application.DTOs.Libros;

public sealed record BookCopyDto(
    Guid Id,
    Guid BookId,
    string Barcode,
    Guid? LocationId,
    string? LocationName,
    string Status,
    string? Condition,
    DateTime AcquiredAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
