namespace Biblioteca.Domain.Entities;

public sealed class Estado
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Usuario> Users { get; set; } = [];
}
