namespace Biblioteca.Domain.Entities;

public sealed class TokenRestablecimientoContrasena
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public Usuario? User { get; set; }
}
