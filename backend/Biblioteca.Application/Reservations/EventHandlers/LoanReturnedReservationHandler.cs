using System.Text.Json;
using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Services.Notifications;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Reservations.EventHandlers;

public sealed class LoanReturnedReservationHandler(
    IReservationRepository reservationRepository,
    INotificationRepository notificationRepository,
    IUsuarioRepository usuarioRepository,
    ILibroRepository libroRepository,
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

        var user = await usuarioRepository.GetByIdAsync(next.UserId, ct);
        var book = await libroRepository.GetByIdAsync(evt.BookId, ct);

        var userName = user?.Profile is { } profile
            ? string.IsNullOrWhiteSpace(profile.DisplayName)
                ? $"{profile.FirstName} {profile.LastName}".Trim()
                : profile.DisplayName
            : "Usuario";
        var bookTitle = book?.Title ?? "Libro";

        var (subject, body) = NotificationTemplates.ReservationReady(
            new ReservationReadyData(userName, bookTitle, next.ExpiresAt!.Value));

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = next.UserId,
            Type = NotificationType.RESERVATION_READY,
            Channel = NotificationChannel.IN_APP,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.PENDING,
            CreatedAt = now,
            PayloadJson = JsonSerializer.Serialize(new { reservationId = next.Id, bookId = evt.BookId })
        };
        await notificationRepository.AddAsync(notification, ct);
    }
}
