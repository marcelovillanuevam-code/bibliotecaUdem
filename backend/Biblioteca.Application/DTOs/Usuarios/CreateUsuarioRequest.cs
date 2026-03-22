using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Usuarios;

public sealed class CreateUsuarioRequest
{
    [Required]
    [StringLength(120)]
    public string NombreCompleto { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Matricula { get; init; } = string.Empty;
}
