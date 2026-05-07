using Biblioteca.Application.DTOs.Prestamos;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Loans;

public interface ILoanService
{
    Task<LoanDto> CreateAsync(CreateLoanRequest request, Guid issuedBy, CancellationToken ct);
    Task<LoanDto> RenewAsync(Guid loanId, Guid requestedBy, CancellationToken ct);
    Task<LoanDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<LoanDto>> GetActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyCollection<LoanDto>> GetByUserAsync(Guid userId, LoanStatus? statusFilter, CancellationToken ct);
    Task<IReadOnlyCollection<LoanDto>> GetAllAsync(LoanStatus? statusFilter, CancellationToken ct);
    Task MarkOverdueAsync(CancellationToken ct);
}
