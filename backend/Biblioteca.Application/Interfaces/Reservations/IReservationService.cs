using Biblioteca.Application.DTOs.Reservations;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Reservations;

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(Guid userId, CreateReservationRequest request, CancellationToken ct);
    Task CancelAsync(Guid reservationId, Guid requestedBy, bool isAdmin, CancellationToken ct);
    Task<ReservationDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<ReservationDto>> GetByUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyCollection<ReservationDto>> GetQueueAsync(Guid bookId, CancellationToken ct);
}
