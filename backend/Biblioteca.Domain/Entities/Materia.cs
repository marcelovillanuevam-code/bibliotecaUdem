namespace Biblioteca.Domain.Entities;

public sealed class Materia
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<LibroMateria> Books { get; set; } = [];
}
