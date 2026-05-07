using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class FineRepository(BibliotecaDbContext dbContext) : IFineRepository
{
    public async Task<Fine?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await dbContext.Fines
            .AsNoTracking()
            .Include(f => f.Return)
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyCollection<Fine>> GetByUserAsync(
        Guid userId, FineStatus? statusFilter, CancellationToken ct)
    {
        var query = dbContext.Fines
            .AsNoTracking()
            .Where(f => f.UserId == userId);

        if (statusFilter.HasValue)
            query = query.Where(f => f.Status == statusFilter.Value);

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<bool> HasPendingByUserAsync(Guid userId, CancellationToken ct) =>
        dbContext.Fines.AnyAsync(f => f.UserId == userId && f.Status == FineStatus.PENDING, ct);

    public async Task<Fine> AddAsync(Fine fine, CancellationToken ct)
    {
        dbContext.Fines.Add(fine);
        await dbContext.SaveChangesAsync(ct);
        return await GetByIdAsync(fine.Id, ct)
            ?? throw new InvalidOperationException("No se pudo recuperar la multa recién creada.");
    }

    public async Task UpdateAsync(Fine fine, CancellationToken ct)
    {
        dbContext.Fines.Update(fine);
        await dbContext.SaveChangesAsync(ct);
    }
}
