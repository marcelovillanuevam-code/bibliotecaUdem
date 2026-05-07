using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<Loan?> GetActiveByBookCopyAsync(Guid bookCopyId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Loan>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Loan>> GetByUserAsync(
        Guid userId,
        string? statusFilter,
        CancellationToken cancellationToken);
    Task<int> CountActiveByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<Loan> AddAsync(Loan loan, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
