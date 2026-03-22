using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.DTOs.Usuarios;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Mappings;

public static class EntityMappings
{
    public static UsuarioDto ToDto(this Usuario usuario) =>
        new(
            usuario.Id,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Matricula,
            usuario.FechaRegistro,
            usuario.Activo);

    public static LibroDto ToDto(this Libro libro) =>
        new(
            libro.Id,
            libro.Titulo,
            libro.Autor,
            libro.Isbn,
            libro.Stock,
            libro.FechaRegistro,
            libro.Disponible);
}
