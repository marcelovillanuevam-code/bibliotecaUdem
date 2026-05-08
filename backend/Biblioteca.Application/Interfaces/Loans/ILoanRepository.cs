using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Loans;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Loan?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<Loan?> GetActiveByBookCopyAsync(Guid bookCopyId, CancellationToken ct);
    Task<IReadOnlyCollection<Loan>> GetActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyCollection<Loan>> GetByUserAsync(Guid userId, LoanStatus? statusFilter, CancellationToken ct);
    Task<IReadOnlyCollection<Loan>> GetAllAsync(LoanStatus? statusFilter, CancellationToken ct, string? copyBarcode = null);
    Task<Loan> AddAsync(Loan loan, CancellationToken ct);
    Task UpdateAsync(Loan loan, CancellationToken ct);
    Task<int> CountActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyCollection<Loan>> GetOverdueActiveAsync(CancellationToken ct);
}
