using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class LoanRepository(BibliotecaDbContext dbContext) : ILoanRepository
{
    private static IQueryable<Loan> WithFullIncludes(IQueryable<Loan> query) =>
        query
            .Include(l => l.User).ThenInclude(u => u!.Profile)
            .Include(l => l.BookCopy).ThenInclude(bc => bc!.Book)
            .Include(l => l.Renewals);

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct) =>
        WithFullIncludes(dbContext.Loans.AsNoTracking())
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) =>
        WithFullIncludes(dbContext.Loans)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<Loan?> GetActiveByBookCopyAsync(Guid bookCopyId, CancellationToken ct) =>
        dbContext.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.BookCopyId == bookCopyId && l.Status == LoanStatus.ACTIVE, ct);

    public async Task<IReadOnlyCollection<Loan>> GetActiveByUserAsync(Guid userId, CancellationToken ct) =>
        await WithFullIncludes(dbContext.Loans.AsNoTracking())
            .Where(l => l.UserId == userId && l.Status == LoanStatus.ACTIVE)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<Loan>> GetByUserAsync(Guid userId, LoanStatus? statusFilter, CancellationToken ct)
    {
        var query = WithFullIncludes(dbContext.Loans.AsNoTracking())
            .Where(l => l.UserId == userId);

        if (statusFilter.HasValue)
            query = query.Where(l => l.Status == statusFilter.Value);

        return await query.OrderByDescending(l => l.LoanedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Loan>> GetAllAsync(LoanStatus? statusFilter, CancellationToken ct)
    {
        var query = WithFullIncludes(dbContext.Loans.AsNoTracking());

        if (statusFilter.HasValue)
            query = query.Where(l => l.Status == statusFilter.Value);

        return await query.OrderByDescending(l => l.LoanedAt).ToListAsync(ct);
    }

    public async Task<Loan> AddAsync(Loan loan, CancellationToken ct)
    {
        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(ct);
        return loan;
    }

    public async Task UpdateAsync(Loan loan, CancellationToken ct)
    {
        dbContext.Loans.Update(loan);
        await dbContext.SaveChangesAsync(ct);
    }

    public Task<int> CountActiveByUserAsync(Guid userId, CancellationToken ct) =>
        dbContext.Loans
            .CountAsync(l => l.UserId == userId && l.Status == LoanStatus.ACTIVE, ct);

    public async Task<IReadOnlyCollection<Loan>> GetOverdueActiveAsync(CancellationToken ct) =>
        await dbContext.Loans
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.ACTIVE && l.DueAt < DateTime.UtcNow)
            .ToListAsync(ct);
}
