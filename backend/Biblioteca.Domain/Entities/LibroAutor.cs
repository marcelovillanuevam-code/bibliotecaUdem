namespace Biblioteca.Domain.Entities;

public sealed class LibroAutor
{
    public Guid BookId { get; set; }
    public Guid AuthorId { get; set; }
    public string? Contribution { get; set; }

    public Libro? Book { get; set; }
    public Autor? Author { get; set; }
}
