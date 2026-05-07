using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Returns;

public interface IReturnRepository
{
    Task<Return?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Return?> GetByLoanAsync(Guid loanId, CancellationToken ct);
    Task<Return> AddAsync(Return returnEntity, CancellationToken ct);
}
