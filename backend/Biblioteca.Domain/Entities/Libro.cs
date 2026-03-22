namespace Biblioteca.Domain.Entities;

public sealed class Libro
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Isbn { get; set; }
    public string? Publisher { get; set; }
    public short? PublicationYear { get; set; }
    public string? Edition { get; set; }
    public string Language { get; set; } = "es";
    public string? SummaryJson { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LibroAutor> Authors { get; set; } = [];
    public ICollection<LibroMateria> Subjects { get; set; } = [];
}
