using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Application.Services.Notifications;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Notifications.EventHandlers;

public sealed class FineCreatedHandler(
    INotificationRepository notificationRepository,
    IDateTimeProvider clock) : IDomainEventHandler<FineCreated>
{
    public async Task HandleAsync(FineCreated evt, CancellationToken ct)
    {
        var (subject, body) = NotificationTemplates.FineCreated(
            new FineCreatedData("Usuario", evt.Reason, evt.Amount));

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = evt.UserId,
            Type = NotificationType.FINE_CREATED,
            Channel = NotificationChannel.IN_APP,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.PENDING,
            CreatedAt = clock.UtcNow,
            PayloadJson = $"{{\"fineId\":\"{evt.FineId}\"}}"
        };
        await notificationRepository.AddAsync(notification, ct);
    }
}
