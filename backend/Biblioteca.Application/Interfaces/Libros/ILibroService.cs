using Biblioteca.Application.DTOs.Libros;

namespace Biblioteca.Application.Interfaces.Libros;

public interface ILibroService
{
    Task<IReadOnlyCollection<LibroDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<LibroDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LibroDto> CreateAsync(CreateLibroRequest request, CancellationToken cancellationToken);
}
