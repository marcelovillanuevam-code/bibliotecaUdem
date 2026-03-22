using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class UsuarioRepository(BibliotecaDbContext dbContext) : IUsuarioRepository
{
    public async Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(usuario => usuario.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<Usuario> AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);
        return usuario;
    }
}
