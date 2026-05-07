using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Auth;

public sealed class RegisterUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    [RegularExpression(
        @"^(?=.*[A-Za-z])(?=.*\d).{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, incluyendo letras y números.")]
    public string Password { get; init; } = string.Empty;

    [StringLength(10)]
    public string PreferredLocale { get; init; } = "es_MX";
}
