using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Libros;

public sealed class CreateLibroRequest
{
    [Required]
    [StringLength(500)]
    public string Title { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Subtitle { get; init; }

    [StringLength(50)]
    public string? Isbn { get; init; }

    [StringLength(255)]
    public string? Publisher { get; init; }

    [Range(0, 32767)]
    public short? PublicationYear { get; init; }

    [StringLength(100)]
    public string? Edition { get; init; }

    [StringLength(50)]
    public string Language { get; init; } = "es";

    public string? SummaryJson { get; init; }
    public string? MetadataJson { get; init; }
}
