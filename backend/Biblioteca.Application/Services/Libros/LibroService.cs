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
        var libro = new Libro
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo.Trim(),
            Autor = request.Autor.Trim(),
            Isbn = request.Isbn.Trim(),
            Stock = request.Stock,
            FechaRegistro = dateTimeProvider.UtcNow,
            Disponible = request.Stock > 0
        };

        var createdLibro = await libroRepository.AddAsync(libro, cancellationToken);

        return createdLibro.ToDto();
    }
}
