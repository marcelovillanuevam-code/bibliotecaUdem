namespace Biblioteca.Domain.Entities;

public sealed class Libro
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Stock { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Disponible { get; set; }
}
