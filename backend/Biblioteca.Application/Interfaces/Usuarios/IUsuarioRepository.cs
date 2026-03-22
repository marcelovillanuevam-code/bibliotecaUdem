using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Usuarios;

public interface IUsuarioRepository
{
    Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken);
}
