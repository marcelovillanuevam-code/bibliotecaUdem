using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [StringLength(100)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;
}
