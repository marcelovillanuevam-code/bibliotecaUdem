using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Libros;

public sealed class CreateLibroAuthorRequest
{
    [Required]
    [StringLength(255)]
    public string FullName { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Contribution { get; init; }
}
