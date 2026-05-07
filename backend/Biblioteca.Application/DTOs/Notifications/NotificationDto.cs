namespace Biblioteca.Application.DTOs.Notifications;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Channel,
    string Subject,
    string Body,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt);
