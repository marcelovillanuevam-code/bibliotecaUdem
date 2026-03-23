using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Usuarios;

public interface IUsuarioRepository
{
    Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Usuario?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken, Guid? excludedUserId = null);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken, Guid? excludedUserId = null);
    Task EnsureReferenceDataAsync(CancellationToken cancellationToken);
    Task<Rol?> GetRoleByCodeAsync(string roleCode, CancellationToken cancellationToken);
    Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken);
    Task<Usuario> RegisterAsync(
        Usuario usuario,
        AutenticacionUsuario auth,
        PerfilUsuario profile,
        ContactoUsuario emailContact,
        UsuarioRol userRole,
        CancellationToken cancellationToken);
    Task UpdateAuthAsync(AutenticacionUsuario auth, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
