using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class LibroRepository(BibliotecaDbContext dbContext) : ILibroRepository
{
    public async Task<IReadOnlyCollection<Libro>> GetAllAsync(GetLibrosRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Books
            .AsNoTracking()
            .Include(libro => libro.Authors)
                .ThenInclude(libroAutor => libroAutor.Author)
            .Include(libro => libro.Subjects)
                .ThenInclude(libroMateria => libroMateria.Subject)
            .AsQueryable();

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

        return await query
            .OrderBy(libro => libro.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Libro?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Books
            .AsNoTracking()
            .Include(libro => libro.Authors)
                .ThenInclude(libroAutor => libroAutor.Author)
            .Include(libro => libro.Subjects)
                .ThenInclude(libroMateria => libroMateria.Subject)
            .AsNoTracking()
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
        dbContext.Books.Remove(libro);
    }
}
