using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Libros;

public interface ILibroRepository
{
    Task<IReadOnlyCollection<Libro>> GetAllAsync(GetLibrosRequest request, CancellationToken cancellationToken);
    Task<Libro?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Libro?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken, Guid? excludedBookId = null);
    Task<Autor?> GetAuthorByFullNameAsync(string fullName, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Materia>> GetSubjectsByCodesAsync(
        IReadOnlyCollection<string> subjectCodes,
        CancellationToken cancellationToken);
    Task<Libro> AddAsync(Libro libro, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    void Remove(Libro libro);
}
