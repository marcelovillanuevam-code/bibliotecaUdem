namespace Biblioteca.Application.DTOs.Libros;

public sealed record LibroDto(
    Guid Id,
    string Titulo,
    string Autor,
    string Isbn,
    int Stock,
    DateTime FechaRegistro,
    bool Disponible);
