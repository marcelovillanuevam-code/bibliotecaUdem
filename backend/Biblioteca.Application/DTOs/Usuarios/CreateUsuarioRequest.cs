using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Usuarios;

public sealed class CreateUsuarioRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [StringLength(160)]
    public string? DisplayName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string RoleCode { get; init; } = "STUDENT";

    [StringLength(30)]
    public string StatusCode { get; init; } = "active";

    [StringLength(10)]
    public string PreferredLocale { get; init; } = "es_MX";

    [StringLength(40)]
    public string? DocumentType { get; init; }

    [StringLength(80)]
    public string? DocumentNumber { get; init; }

    public string? MetadataJson { get; init; }
}
