namespace Biblioteca.Application.DTOs.Libros;

public sealed record LibroAutorDto(
    Guid Id,
    string FullName,
    string? Contribution);
