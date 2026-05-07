using Biblioteca.Application.DTOs.Notifications;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Reservations;

public interface INotificationService
{
    Task EnqueueAsync(Guid userId, NotificationType type, NotificationChannel channel,
        string subject, string body, object payload, CancellationToken ct);
    Task<IReadOnlyCollection<NotificationDto>> GetInboxAsync(Guid userId, bool unreadOnly, CancellationToken ct);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct);
}
