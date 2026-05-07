namespace Biblioteca.Domain.Entities;

public enum NotificationType { LOAN_RECEIPT, RETURN_RECEIPT, FINE_CREATED, RESERVATION_READY, DUE_REMINDER }
public enum NotificationChannel { EMAIL, WHATSAPP, IN_APP }
public enum NotificationStatus { PENDING, SENT, FAILED }

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = "{}";

    public Usuario? User { get; set; }
}
