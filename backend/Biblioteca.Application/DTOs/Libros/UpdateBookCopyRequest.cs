using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Libros;

public sealed class UpdateBookCopyRequest
{
    [Required]
    [StringLength(50)]
    public string Status { get; init; } = string.Empty;

    public Guid? LocationId { get; init; }

    [StringLength(50)]
    public string? Condition { get; init; }
}
