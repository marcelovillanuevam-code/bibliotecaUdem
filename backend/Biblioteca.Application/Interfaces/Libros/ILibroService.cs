using Biblioteca.Application.DTOs.Libros;

namespace Biblioteca.Application.Interfaces.Libros;

public interface ILibroService
{
    Task<IReadOnlyCollection<LibroDto>> GetAllAsync(GetLibrosRequest request, CancellationToken cancellationToken);
    Task<LibroFichaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LibroFichaDto> CreateAsync(CreateLibroRequest request, CancellationToken cancellationToken);
    Task<LibroFichaDto> UpdateAsync(Guid id, UpdateLibroRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
