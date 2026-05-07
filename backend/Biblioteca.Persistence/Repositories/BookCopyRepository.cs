using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class BookCopyRepository(
    BibliotecaDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : IBookCopyRepository
{
    public async Task<BookCopy?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.BookCopies
            .AsNoTracking()
            .Include(bc => bc.Location)
            .FirstOrDefaultAsync(bc => bc.Id == id, cancellationToken);

    public async Task<BookCopy?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.BookCopies
            .Include(bc => bc.Location)
            .FirstOrDefaultAsync(bc => bc.Id == id, cancellationToken);

    public async Task<BookCopy?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken) =>
        await dbContext.BookCopies
            .AsNoTracking()
            .FirstOrDefaultAsync(bc => bc.Barcode == barcode, cancellationToken);

    public async Task<IReadOnlyCollection<BookCopy>> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken) =>
        await dbContext.BookCopies
            .AsNoTracking()
            .Include(bc => bc.Location)
            .Where(bc => bc.BookId == bookId)
            .OrderBy(bc => bc.Barcode)
            .ToListAsync(cancellationToken);

    public Task<bool> BarcodeExistsAsync(string barcode, CancellationToken cancellationToken, Guid? excludedCopyId = null) =>
        dbContext.BookCopies.AnyAsync(
            bc => bc.Barcode == barcode &&
                  (excludedCopyId == null || bc.Id != excludedCopyId.Value),
            cancellationToken);

    public Task<bool> HasActiveCopiesAsync(Guid bookId, CancellationToken cancellationToken) =>
        dbContext.BookCopies.AnyAsync(bc => bc.BookId == bookId, cancellationToken);

    public async Task<BookCopy> AddAsync(BookCopy copy, CancellationToken cancellationToken)
    {
        dbContext.BookCopies.Add(copy);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(copy.Id, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el ejemplar recién creado.");
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public void SoftDelete(BookCopy copy)
    {
        var now = dateTimeProvider.UtcNow;
        copy.DeletedAt = now;
        copy.UpdatedAt = now;
        dbContext.BookCopies.Update(copy);
    }

    public Task<int> CountAvailableByBookIdAsync(Guid bookId, CancellationToken cancellationToken) =>
        dbContext.BookCopies
            .CountAsync(bc => bc.BookId == bookId && bc.Status == BookCopyStatus.Available, cancellationToken);
}
