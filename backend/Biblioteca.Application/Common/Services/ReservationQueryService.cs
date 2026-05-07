using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Common.Services;

public sealed class ReservationQueryService(IReservationRepository reservationRepository)
    : IReservationQueryService
{
    public async Task<bool> HasActiveReservationsForBookAsync(Guid bookId, CancellationToken ct)
    {
        var next = await reservationRepository.GetNextInQueueAsync(bookId, ct);
        return next is not null;
    }
}
