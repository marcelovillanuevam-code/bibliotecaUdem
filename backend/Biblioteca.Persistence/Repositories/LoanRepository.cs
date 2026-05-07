using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class LoanRepository(BibliotecaDbContext dbContext) : ILoanRepository
{
    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Loans
            .AsNoTracking()
            .Include(loan => loan.User)
            .Include(loan => loan.BookCopy)
            .Include(loan => loan.Renewals)
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

    public Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Loans
            .Include(loan => loan.BookCopy)
            .Include(loan => loan.Renewals)
            .FirstOrDefaultAsync(loan => loan.Id == id, cancellationToken);

    public Task<Loan?> GetActiveByBookCopyAsync(Guid bookCopyId, CancellationToken cancellationToken) =>
        dbContext.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(
                loan => loan.BookCopyId == bookCopyId && loan.Status == LoanStatus.Active,
                cancellationToken);

    public async Task<IReadOnlyCollection<Loan>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Loans
            .AsNoTracking()
            .Include(loan => loan.BookCopy)
            .Where(loan => loan.UserId == userId && loan.Status == LoanStatus.Active)
            .OrderByDescending(loan => loan.LoanedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Loan>> GetByUserAsync(
        Guid userId,
        string? statusFilter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Loans
            .AsNoTracking()
            .Include(loan => loan.BookCopy)
            .Where(loan => loan.UserId == userId);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            var normalized = statusFilter.Trim().ToUpperInvariant();
            query = query.Where(loan => loan.Status == normalized);
        }

        return await query
            .OrderByDescending(loan => loan.LoanedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Loans.CountAsync(
            loan => loan.UserId == userId && loan.Status == LoanStatus.Active,
            cancellationToken);

    public async Task<Loan> AddAsync(Loan loan, CancellationToken cancellationToken)
    {
        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(loan.Id, cancellationToken)
            ?? throw new InvalidOperationException("No se pudo recuperar el préstamo recién creado.");
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
