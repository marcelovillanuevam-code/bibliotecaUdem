namespace Biblioteca.Domain.Entities;

public sealed class Ubicacion
{
    public Guid Id { get; set; }
    public string LibraryName { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Shelf { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
