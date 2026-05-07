using Biblioteca.Application.DTOs.Libros;

namespace Biblioteca.Application.Interfaces.Libros;

public interface IBookCopyService
{
    Task<IReadOnlyCollection<BookCopyDto>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken);
    Task<BookCopyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BookCopyDto> CreateAsync(Guid bookId, CreateBookCopyRequest request, CancellationToken cancellationToken);
    Task<BookCopyDto> UpdateAsync(Guid id, UpdateBookCopyRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
