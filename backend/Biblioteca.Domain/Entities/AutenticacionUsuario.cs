namespace Biblioteca.Domain.Entities;

public sealed class AutenticacionUsuario
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? PasswordChangedAt { get; set; }
    public bool MfaEnabled { get; set; }
    public byte[]? MfaSecret { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Usuario? User { get; set; }
}
