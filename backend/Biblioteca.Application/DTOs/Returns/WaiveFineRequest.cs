using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Returns;

public sealed class WaiveFineRequest
{
    [Required]
    [StringLength(500)]
    public string Reason { get; init; } = string.Empty;
}
