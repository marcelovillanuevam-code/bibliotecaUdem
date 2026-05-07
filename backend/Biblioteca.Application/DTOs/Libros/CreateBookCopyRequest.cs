using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Libros;

public sealed class CreateBookCopyRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Barcode { get; init; } = string.Empty;

    public Guid? LocationId { get; init; }

    [StringLength(50)]
    public string? Status { get; init; }

    [StringLength(50)]
    public string? Condition { get; init; }

    public DateTime AcquiredAt { get; init; }
}
