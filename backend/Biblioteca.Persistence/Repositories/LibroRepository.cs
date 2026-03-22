using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class LibroRepository(BibliotecaDbContext dbContext) : ILibroRepository
{
    public async Task<IReadOnlyCollection<Libro>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Libros
            .AsNoTracking()
            .OrderBy(libro => libro.Titulo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Libro?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Libros
            .AsNoTracking()
            .FirstOrDefaultAsync(libro => libro.Id == id, cancellationToken);
    }

    public async Task<Libro> AddAsync(Libro libro, CancellationToken cancellationToken)
    {
        dbContext.Libros.Add(libro);
        await dbContext.SaveChangesAsync(cancellationToken);
        return libro;
    }
}
