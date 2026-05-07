namespace Biblioteca.Domain.Entities;

public sealed class UsuarioRol
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }

    public Usuario? User { get; set; }
    public Rol? Role { get; set; }
}
