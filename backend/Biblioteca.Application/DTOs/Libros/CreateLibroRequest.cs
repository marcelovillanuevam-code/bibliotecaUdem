using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Application.DTOs.Libros;

public sealed class CreateLibroRequest
{
    [Required]
    [StringLength(150)]
    public string Titulo { get; init; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Autor { get; init; } = string.Empty;

    [Required]
    [StringLength(17)]
    public string Isbn { get; init; } = string.Empty;

    [Range(0, 999)]
    public int Stock { get; init; }
}
