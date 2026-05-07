using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Reservations;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Reservation?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<Reservation>> GetByUserAsync(Guid userId, ReservationStatus? statusFilter, CancellationToken ct);
    Task<IReadOnlyCollection<Reservation>> GetByBookAsync(Guid bookId, ReservationStatus? statusFilter, CancellationToken ct);
    Task<Reservation?> GetActiveByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken ct);
    Task<Reservation?> GetReadyByUserAndBookForUpdateAsync(Guid userId, Guid bookId, CancellationToken ct);
    Task<Reservation?> GetNextInQueueAsync(Guid bookId, CancellationToken ct);
    Task<IReadOnlyCollection<Reservation>> GetExpiredReadyAsync(CancellationToken ct);
    Task<int> GetMaxQueuePositionAsync(Guid bookId, CancellationToken ct);
    Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct);
    Task UpdateAsync(Reservation reservation, CancellationToken ct);
}
