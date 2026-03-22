namespace Biblioteca.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UsernameLower { get; private set; }
    public string StatusCode { get; set; } = "pending_verification";
    public string PreferredLocale { get; set; } = "es_MX";
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Estado? Status { get; set; }
    public AutenticacionUsuario? Auth { get; set; }
    public PerfilUsuario? Profile { get; set; }
    public ICollection<ContactoUsuario> Contacts { get; set; } = [];
    public ICollection<UsuarioRol> Roles { get; set; } = [];
    public ICollection<Sesion> Sessions { get; set; } = [];
    public ICollection<TokenRestablecimientoContrasena> PasswordResetTokens { get; set; } = [];
}
