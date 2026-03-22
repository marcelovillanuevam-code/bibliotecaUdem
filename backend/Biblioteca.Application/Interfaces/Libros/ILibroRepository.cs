using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Libros;

public interface ILibroRepository
{
    Task<IReadOnlyCollection<Libro>> GetAllAsync(CancellationToken cancellationToken);
    Task<Libro?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Libro> AddAsync(Libro libro, CancellationToken cancellationToken);
}
