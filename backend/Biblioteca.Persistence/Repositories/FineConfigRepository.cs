using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class FineConfigRepository(BibliotecaDbContext dbContext) : IFineConfigRepository
{
    public async Task<FineConfig?> GetActiveAsync(CancellationToken ct) =>
        await dbContext.FineConfigs
            .AsNoTracking()
            .Where(fc => fc.EffectiveFrom <= DateTime.UtcNow)
            .OrderByDescending(fc => fc.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

    public async Task<FineConfig> AddAsync(FineConfig config, CancellationToken ct)
    {
        dbContext.FineConfigs.Add(config);
        await dbContext.SaveChangesAsync(ct);
        return config;
    }
}
