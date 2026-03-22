using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Mappings;

public static class EntityMappings
{
    public static UsuarioDto ToDto(this Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Username,
            usuario.StatusCode,
            usuario.PreferredLocale,
            usuario.CreatedAt,
            usuario.UpdatedAt,
            usuario.DeletedAt);

    public static LibroDto ToDto(this Libro libro) =>
        new(
            libro.Id,
            libro.Title,
            libro.Subtitle,
            libro.Isbn,
            libro.Publisher,
            libro.PublicationYear,
            libro.Edition,
            libro.Language,
            libro.CreatedAt,
            libro.UpdatedAt);
}
