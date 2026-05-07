using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Libros;

public interface IBookCopyRepository
{
    Task<BookCopy?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BookCopy?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<BookCopy?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BookCopy>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken);
    Task<bool> BarcodeExistsAsync(string barcode, CancellationToken cancellationToken, Guid? excludedCopyId = null);
    Task<bool> HasActiveCopiesAsync(Guid bookId, CancellationToken cancellationToken);
    Task<int> CountAvailableByBookIdAsync(Guid bookId, CancellationToken cancellationToken);
    Task<BookCopy> AddAsync(BookCopy copy, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    void SoftDelete(BookCopy copy);
}
