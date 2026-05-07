using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Returns;

public interface IFineRepository
{
    Task<Fine?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<Fine>> GetByUserAsync(Guid userId, FineStatus? statusFilter, CancellationToken ct);
    Task<bool> HasPendingByUserAsync(Guid userId, CancellationToken ct);
    Task<Fine> AddAsync(Fine fine, CancellationToken ct);
    Task UpdateAsync(Fine fine, CancellationToken ct);
}
