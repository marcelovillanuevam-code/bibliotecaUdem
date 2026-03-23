using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Libros;

public sealed class LibroService(
    ILibroRepository libroRepository,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) : ILibroService
{
    public async Task<IReadOnlyCollection<LibroDto>> GetAllAsync(GetLibrosRequest request, CancellationToken cancellationToken)
    {
        var libros = await libroRepository.GetAllAsync(request, cancellationToken);
        return mapper.Map<IReadOnlyCollection<LibroDto>>(libros);
    }

    public async Task<LibroFichaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var libro = await libroRepository.GetByIdAsync(id, cancellationToken);
        return libro is null ? null : mapper.Map<LibroFichaDto>(libro);
    }

    public async Task<LibroFichaDto> CreateAsync(CreateLibroRequest request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var normalizedIsbn = await EnsureIsbnAvailableAsync(request.Isbn, null, cancellationToken);
        var (subjects, _) = await ResolveSubjectsAsync(request.SubjectCodes, cancellationToken);

        var libro = new Libro
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Subtitle = string.IsNullOrWhiteSpace(request.Subtitle) ? null : request.Subtitle.Trim(),
            Isbn = normalizedIsbn,
            Publisher = string.IsNullOrWhiteSpace(request.Publisher) ? null : request.Publisher.Trim(),
            PublicationYear = request.PublicationYear,
            Edition = string.IsNullOrWhiteSpace(request.Edition) ? null : request.Edition.Trim(),
            Language = string.IsNullOrWhiteSpace(request.Language) ? "es" : request.Language.Trim(),
            SummaryJson = string.IsNullOrWhiteSpace(request.SummaryJson) ? null : request.SummaryJson.Trim(),
            MetadataJson = string.IsNullOrWhiteSpace(request.MetadataJson) ? null : request.MetadataJson.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await AssignAuthorsAsync(libro, request.Authors, now, cancellationToken);
        AssignSubjects(libro, subjects);

        var createdLibro = await libroRepository.AddAsync(libro, cancellationToken);

        return mapper.Map<LibroFichaDto>(createdLibro);
    }

    public async Task<LibroFichaDto> UpdateAsync(Guid id, UpdateLibroRequest request, CancellationToken cancellationToken)
    {
        var libro = await libroRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontro el libro solicitado.");

        var now = dateTimeProvider.UtcNow;
        var normalizedIsbn = await EnsureIsbnAvailableAsync(request.Isbn, id, cancellationToken);
        var (subjects, _) = await ResolveSubjectsAsync(request.SubjectCodes, cancellationToken);

        libro.Title = request.Title.Trim();
        libro.Subtitle = NormalizeOptional(request.Subtitle);
        libro.Isbn = normalizedIsbn;
        libro.Publisher = NormalizeOptional(request.Publisher);
        libro.PublicationYear = request.PublicationYear;
        libro.Edition = NormalizeOptional(request.Edition);
        libro.Language = NormalizeLanguage(request.Language);
        libro.SummaryJson = NormalizeOptional(request.SummaryJson);
        libro.MetadataJson = NormalizeOptional(request.MetadataJson);
        libro.UpdatedAt = now;

        libro.Authors.Clear();
        libro.Subjects.Clear();

        await AssignAuthorsAsync(libro, request.Authors, now, cancellationToken);
        AssignSubjects(libro, subjects);

        await libroRepository.SaveChangesAsync(cancellationToken);

        var updatedLibro = await libroRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el libro actualizado.");

        return mapper.Map<LibroFichaDto>(updatedLibro);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var libro = await libroRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontro el libro solicitado.");

        libroRepository.Remove(libro);
        await libroRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> EnsureIsbnAvailableAsync(
        string? isbn,
        Guid? excludedBookId,
        CancellationToken cancellationToken)
    {
        var normalizedIsbn = NormalizeOptional(isbn);
        if (normalizedIsbn is null)
        {
            return null;
        }

        if (await libroRepository.IsbnExistsAsync(normalizedIsbn, cancellationToken, excludedBookId))
        {
            throw new ConflictException("El ISBN ya esta registrado.");
        }

        return normalizedIsbn;
    }

    private async Task<(IReadOnlyCollection<Materia> Subjects, string[] NormalizedCodes)> ResolveSubjectsAsync(
        IReadOnlyCollection<string> subjectCodes,
        CancellationToken cancellationToken)
    {
        var normalizedSubjectCodes = subjectCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedSubjectCodes.Length == 0)
        {
            throw new ValidationException("Debe especificar al menos una materia valida.");
        }

        var subjects = await libroRepository.GetSubjectsByCodesAsync(normalizedSubjectCodes, cancellationToken);
        var missingSubjectCodes = normalizedSubjectCodes
            .Except(subjects.Select(subject => subject.Code), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingSubjectCodes.Length > 0)
        {
            throw new NotFoundException(
                $"No se encontraron las materias: {string.Join(", ", missingSubjectCodes)}.");
        }

        return (subjects, normalizedSubjectCodes);
    }

    private async Task AssignAuthorsAsync(
        Libro libro,
        IReadOnlyCollection<CreateLibroAuthorRequest> authorRequests,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalizedAuthors = authorRequests
            .Where(author => !string.IsNullOrWhiteSpace(author.FullName))
            .GroupBy(author => author.FullName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        if (normalizedAuthors.Length == 0)
        {
            throw new ValidationException("Debe especificar al menos un autor valido.");
        }

        foreach (var authorRequest in normalizedAuthors)
        {
            var normalizedFullName = authorRequest.FullName.Trim();
            var existingAuthor = await libroRepository.GetAuthorByFullNameAsync(normalizedFullName, cancellationToken);

            var author = existingAuthor ?? new Autor
            {
                Id = Guid.NewGuid(),
                FullName = normalizedFullName,
                CreatedAt = now,
                UpdatedAt = now
            };

            libro.Authors.Add(new LibroAutor
            {
                BookId = libro.Id,
                AuthorId = author.Id,
                Contribution = NormalizeOptional(authorRequest.Contribution),
                Author = author
            });
        }
    }

    private static void AssignSubjects(Libro libro, IReadOnlyCollection<Materia> subjects)
    {
        foreach (var subject in subjects)
        {
            libro.Subjects.Add(new LibroMateria
            {
                BookId = libro.Id,
                SubjectId = subject.Id,
                Subject = subject
            });
        }
    }

    private static string NormalizeLanguage(string? language) =>
        string.IsNullOrWhiteSpace(language) ? "es" : language.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
