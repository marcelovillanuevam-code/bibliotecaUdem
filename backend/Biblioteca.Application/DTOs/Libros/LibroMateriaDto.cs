namespace Biblioteca.Application.DTOs.Libros;

public sealed record LibroMateriaDto(
    Guid Id,
    string Code,
    string Name);
