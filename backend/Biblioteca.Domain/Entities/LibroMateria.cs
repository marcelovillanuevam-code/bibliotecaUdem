namespace Biblioteca.Domain.Entities;

public sealed class LibroMateria
{
    public Guid BookId { get; set; }
    public Guid SubjectId { get; set; }

    public Libro? Book { get; set; }
    public Materia? Subject { get; set; }
}
