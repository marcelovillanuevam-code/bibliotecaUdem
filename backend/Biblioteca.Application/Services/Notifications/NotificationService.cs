using System.Text.Json;
using Biblioteca.Application.DTOs.Notifications;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Notifications;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    IDateTimeProvider clock) : INotificationService
{
    public async Task EnqueueAsync(Guid userId, NotificationType type, NotificationChannel channel,
        string subject, string body, object payload, CancellationToken ct)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Channel = channel,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.PENDING,
            CreatedAt = clock.UtcNow,
            PayloadJson = JsonSerializer.Serialize(payload)
        };
        await notificationRepository.AddAsync(notification, ct);
    }

    public async Task<IReadOnlyCollection<NotificationDto>> GetInboxAsync(Guid userId, bool unreadOnly, CancellationToken ct)
    {
        var notifications = await notificationRepository.GetByUserAsync(userId, unreadOnly, ct);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct)
    {
        var pending = await notificationRepository.GetByUserAsync(userId, false, ct);
        var notification = pending.FirstOrDefault(n => n.Id == notificationId)
            ?? throw new NotFoundException("Notificación no encontrada.");

        notification.Status = NotificationStatus.SENT;
        notification.SentAt = clock.UtcNow;
        await notificationRepository.UpdateAsync(notification, ct);
    }

    private static NotificationDto MapToDto(Notification n) =>
        new(n.Id, n.UserId, n.Type.ToString(), n.Channel.ToString(), n.Subject, n.Body, n.Status.ToString(), n.CreatedAt, n.SentAt);
}
