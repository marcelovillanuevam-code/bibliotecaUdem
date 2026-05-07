using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Returns;

public sealed class ConfirmPaymentRequest
{
    [Required]
    [StringLength(30)]
    public string Method { get; init; } = string.Empty;

    [StringLength(200)]
    public string? Reference { get; init; }
}
