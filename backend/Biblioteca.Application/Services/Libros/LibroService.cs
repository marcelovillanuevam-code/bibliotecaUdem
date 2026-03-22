using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Mappings;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Libros;

public sealed class LibroService(
    ILibroRepository libroRepository,
    IDateTimeProvider dateTimeProvider) : ILibroService
{
    public async Task<IReadOnlyCollection<LibroDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var libros = await libroRepository.GetAllAsync(cancellationToken);
        return libros.Select(libro => libro.ToDto()).ToArray();
    }

    public async Task<LibroDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var libro = await libroRepository.GetByIdAsync(id, cancellationToken);
        return libro?.ToDto();
    }

    public async Task<LibroDto> CreateAsync(CreateLibroRequest request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var libro = new Libro
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Subtitle = string.IsNullOrWhiteSpace(request.Subtitle) ? null : request.Subtitle.Trim(),
            Isbn = string.IsNullOrWhiteSpace(request.Isbn) ? null : request.Isbn.Trim(),
            Publisher = string.IsNullOrWhiteSpace(request.Publisher) ? null : request.Publisher.Trim(),
            PublicationYear = request.PublicationYear,
            Edition = string.IsNullOrWhiteSpace(request.Edition) ? null : request.Edition.Trim(),
            Language = string.IsNullOrWhiteSpace(request.Language) ? "es" : request.Language.Trim(),
            SummaryJson = string.IsNullOrWhiteSpace(request.SummaryJson) ? null : request.SummaryJson.Trim(),
            MetadataJson = string.IsNullOrWhiteSpace(request.MetadataJson) ? null : request.MetadataJson.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var createdLibro = await libroRepository.AddAsync(libro, cancellationToken);

        return createdLibro.ToDto();
    }
}
