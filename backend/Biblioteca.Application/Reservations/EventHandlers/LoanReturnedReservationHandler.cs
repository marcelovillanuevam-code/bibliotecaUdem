using System.Text.Json;
using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Reservations.EventHandlers;

public sealed class LoanReturnedReservationHandler(
    IReservationRepository reservationRepository,
    INotificationRepository notificationRepository,
    IDateTimeProvider clock) : IDomainEventHandler<LoanReturned>
{
    private static readonly TimeSpan ReadyWindow = TimeSpan.FromHours(48);

    public async Task HandleAsync(LoanReturned evt, CancellationToken ct)
    {
        if (evt.Condition == ReturnCondition.LOST) return;

        var next = await reservationRepository.GetNextInQueueAsync(evt.BookId, ct);
        if (next is null) return;

        var now = clock.UtcNow;
        next.Status = ReservationStatus.READY;
        next.ReadyAt = now;
        next.ExpiresAt = now.Add(ReadyWindow);

        await reservationRepository.UpdateAsync(next, ct);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = next.UserId,
            Type = NotificationType.RESERVATION_READY,
            Channel = NotificationChannel.IN_APP,
            Subject = "Tu reserva está lista",
            Body = "El libro que reservaste está disponible. Tienes 48h para retirarlo.",
            Status = NotificationStatus.PENDING,
            CreatedAt = now,
            PayloadJson = JsonSerializer.Serialize(new { reservationId = next.Id, bookId = evt.BookId })
        };
        await notificationRepository.AddAsync(notification, ct);
    }
}
