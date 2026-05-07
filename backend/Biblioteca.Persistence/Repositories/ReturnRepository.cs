using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class ReturnRepository(BibliotecaDbContext dbContext) : IReturnRepository
{
    public async Task<List<Return>> ListAsync(CancellationToken ct) =>
        await dbContext.Returns
            .AsNoTracking()
            .Include(r => r.Fine)
            .OrderByDescending(r => r.ReturnedAt)
            .ToListAsync(ct);

    public async Task<Return?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await dbContext.Returns
            .AsNoTracking()
            .Include(r => r.Loan)
            .Include(r => r.Fine)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Return?> GetByLoanAsync(Guid loanId, CancellationToken ct) =>
        await dbContext.Returns
            .AsNoTracking()
            .Include(r => r.Fine)
            .FirstOrDefaultAsync(r => r.LoanId == loanId, ct);

    public async Task<Return> AddAsync(Return returnEntity, CancellationToken ct)
    {
        dbContext.Returns.Add(returnEntity);
        await dbContext.SaveChangesAsync(ct);
        return await GetByIdAsync(returnEntity.Id, ct)
            ?? throw new InvalidOperationException("No se pudo recuperar la devolución recién registrada.");
    }
}
