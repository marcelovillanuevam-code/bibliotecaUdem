namespace Biblioteca.Application.DTOs.Usuarios;

public sealed record UsuarioDto(
    Guid Id,
    string Username,
    string StatusCode,
    string PreferredLocale,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt);
