namespace Biblioteca.Domain.Entities;

public sealed class Rol
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<UsuarioRol> Users { get; set; } = [];
}
