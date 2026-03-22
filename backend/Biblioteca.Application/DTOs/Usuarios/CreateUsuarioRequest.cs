using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Usuarios;

public sealed class CreateUsuarioRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [StringLength(30)]
    public string StatusCode { get; init; } = "pending_verification";

    [StringLength(10)]
    public string PreferredLocale { get; init; } = "es_MX";

    public string? MetadataJson { get; init; }
}
