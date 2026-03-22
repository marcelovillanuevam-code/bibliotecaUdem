namespace Biblioteca.Application.DTOs.Usuarios;

public sealed record UsuarioDto(
    Guid Id,
    string NombreCompleto,
    string Email,
    string Matricula,
    DateTime FechaRegistro,
    bool Activo);
