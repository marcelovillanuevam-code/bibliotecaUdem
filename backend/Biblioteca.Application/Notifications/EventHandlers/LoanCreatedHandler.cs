using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Services.Notifications;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Notifications.EventHandlers;

public sealed class LoanCreatedHandler(
    INotificationRepository notificationRepository,
    IUsuarioRepository usuarioRepository,
    ILibroRepository libroRepository,
    IDateTimeProvider clock) : IDomainEventHandler<LoanCreated>
{
    public async Task HandleAsync(LoanCreated evt, CancellationToken ct)
    {
        var user = await usuarioRepository.GetByIdAsync(evt.UserId, ct);
        var book = await libroRepository.GetByIdAsync(evt.BookId, ct);

        var userName = user?.Profile is { } profile
            ? string.IsNullOrWhiteSpace(profile.DisplayName)
                ? $"{profile.FirstName} {profile.LastName}".Trim()
                : profile.DisplayName
            : "Usuario";
        var bookTitle = book?.Title ?? "Libro";

        var (subject, body) = NotificationTemplates.LoanReceipt(
            new LoanReceiptData(userName, bookTitle, evt.DueDate));

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = evt.UserId,
            Type = NotificationType.LOAN_RECEIPT,
            Channel = NotificationChannel.IN_APP,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.PENDING,
            CreatedAt = clock.UtcNow,
            PayloadJson = $"{{\"loanId\":\"{evt.LoanId}\"}}"
        };
        await notificationRepository.AddAsync(notification, ct);
    }
}
