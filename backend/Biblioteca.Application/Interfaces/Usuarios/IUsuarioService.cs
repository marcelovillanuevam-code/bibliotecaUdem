using Biblioteca.Application.DTOs.Usuarios;

namespace Biblioteca.Application.Interfaces.Usuarios;

public interface IUsuarioService
{
    Task<IReadOnlyCollection<UsuarioDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<UsuarioDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UsuarioDto> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken);
}
