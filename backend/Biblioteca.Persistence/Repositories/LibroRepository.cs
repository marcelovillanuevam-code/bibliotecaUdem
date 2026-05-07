using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class LibroRepository(
    BibliotecaDbContext dbContext,
    IDateTimeProvider dateTimeProvider) : ILibroRepository
{
    public async Task<IReadOnlyCollection<LibroDto>> GetAllAsync(GetLibrosRequest request, CancellationToken cancellationToken)
    {
        // Query 1: libros con autores y materias (HasQueryFilter excluye deleted_at != null automáticamente)
        IQueryable<Libro> query = dbContext.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = request.Title.Trim();
            query = query.Where(libro => EF.Functions.ILike(libro.Title, $"%{title}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            var author = request.Author.Trim();
            query = query.Where(libro =>
                libro.Authors.Any(libroAutor =>
                    libroAutor.Author != null &&
                    EF.Functions.ILike(libroAutor.Author.FullName, $"%{author}%")));
        }

        if (!string.IsNullOrWhiteSpace(request.Subject))
        {
            var subject = request.Subject.Trim();
            query = query.Where(libro =>
                libro.Subjects.Any(libroMateria =>
                    libroMateria.Subject != null &&
                    (EF.Functions.ILike(libroMateria.Subject.Code, $"%{subject}%") ||
                     EF.Functions.ILike(libroMateria.Subject.Name, $"%{subject}%"))));
        }

        if (!string.IsNullOrWhiteSpace(request.Isbn))
        {
            var isbn = request.Isbn.Trim();
            query = query.Where(libro => libro.Isbn != null && EF.Functions.ILike(libro.Isbn, $"%{isbn}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Publisher))
        {
            var publisher = request.Publisher.Trim();
            query = query.Where(libro =>
                libro.Publisher != null && EF.Functions.ILike(libro.Publisher, $"%{publisher}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            var language = request.Language.Trim();
            query = query.Where(libro => EF.Functions.ILike(libro.Language, language));
        }

        var libros = await query
            .Include(libro => libro.Authors)
                .ThenInclude(libroAutor => libroAutor.Author)
            .Include(libro => libro.Subjects)
                .ThenInclude(libroMateria => libroMateria.Subject)
            .OrderBy(libro => libro.Title)
            .ToListAsync(cancellationToken);

        if (libros.Count == 0)
            return [];

        // Query 2: conteos de copias agrupados por libro (HasQueryFilter excluye deleted_at != null)
        var bookIds = libros.Select(l => l.Id).ToList();
        var copyCountsList = await dbContext.BookCopies
            .AsNoTracking()
            .Where(bc => bookIds.Contains(bc.BookId))
            .GroupBy(bc => bc.BookId)
            .Select(g => new
            {
                BookId = g.Key,
                Total = g.Count(),
                Available = g.Count(bc => bc.Status == BookCopyStatus.Available)
            })
            .ToListAsync(cancellationToken);

        var copyCounts = copyCountsList.ToDictionary(c => c.BookId);

        return libros.Select(libro =>
        {
            copyCounts.TryGetValue(libro.Id, out var counts);
            return new LibroDto(
                libro.Id,
                libro.Title,
                libro.Subtitle,
                libro.Isbn,
                libro.Publisher,
                libro.PublicationYear,
                libro.Edition,
                libro.Language,
                libro.Authors
                    .OrderBy(la => la.Author != null ? la.Author.FullName : string.Empty)
                    .Select(la => new LibroAutorDto(la.AuthorId, la.Author?.FullName ?? string.Empty, la.Contribution))
                    .ToArray(),
                libro.Subjects
                    .OrderBy(lm => lm.Subject != null ? lm.Subject.Name : string.Empty)
                    .Select(lm => new LibroMateriaDto(lm.SubjectId, lm.Subject?.Code ?? string.Empty, lm.Subject?.Name ?? string.Empty))
                    .ToArray(),
                libro.CreatedAt,
                libro.UpdatedAt,
                counts?.Total ?? 0,
                counts?.Available ?? 0
            );
        }).ToList().AsReadOnly();
    }

    public async Task<Libro?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Include(libro => libro.Authors)
                .ThenInclude(libroAutor => libroAutor.Author)
            .Include(libro => libro.Subjects)
                .ThenInclude(libroMateria => libroMateria.Subject)
            .Include(libro => libro.Copies)
            .FirstOrDefaultAsync(libro => libro.Id == id, cancellationToken);
    }

    public async Task<Libro?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .Include(libro => libro.Authors)
                .ThenInclude(libroAutor => libroAutor.Author)
            .Include(libro => libro.Subjects)
                .ThenInclude(libroMateria => libroMateria.Subject)
            .FirstOrDefaultAsync(libro => libro.Id == id, cancellationToken);
    }

    public Task<bool> IsbnExistsAsync(string isbn, CancellationToken cancellationToken, Guid? excludedBookId = null) =>
        dbContext.Books.AnyAsync(
            libro => libro.Isbn == isbn &&
                     (excludedBookId == null || libro.Id != excludedBookId.Value),
            cancellationToken);

    public Task<Autor?> GetAuthorByFullNameAsync(string fullName, CancellationToken cancellationToken)
    {
        var normalizedFullName = fullName.Trim().ToLowerInvariant();

        return dbContext.Authors.FirstOrDefaultAsync(
            author => author.FullName.ToLower() == normalizedFullName,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<Materia>> GetSubjectsByCodesAsync(
        IReadOnlyCollection<string> subjectCodes,
        CancellationToken cancellationToken)
    {
        var normalizedCodes = subjectCodes
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return await dbContext.Subjects
            .Where(subject => normalizedCodes.Contains(subject.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<Libro> AddAsync(Libro libro, CancellationToken cancellationToken)
    {
        dbContext.Books.Add(libro);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(libro.Id, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el libro recien creado.");
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Remove(Libro libro)
    {
        var now = dateTimeProvider.UtcNow;
        libro.DeletedAt = now;
        libro.UpdatedAt = now;
        dbContext.Books.Update(libro);
    }
}
