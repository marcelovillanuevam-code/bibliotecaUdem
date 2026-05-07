namespace Biblioteca.Domain.Entities;

public sealed class Autor
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? BioJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LibroAutor> Books { get; set; } = [];
}
