namespace Biblioteca.Application.DTOs.Usuarios;

public sealed record UsuarioDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string DisplayName,
    string Email,
    string UniversityId,
    string? DocumentType,
    string? DocumentNumber,
    string RoleCode,
    string RoleLabel,
    string StatusCode,
    string StatusLabel,
    string PreferredLocale,
    string? MetadataJson,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt);
