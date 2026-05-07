using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Application.Services.Notifications;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Notifications.EventHandlers;

public sealed class LoanCreatedHandler(
    INotificationRepository notificationRepository,
    IDateTimeProvider clock) : IDomainEventHandler<LoanCreated>
{
    public async Task HandleAsync(LoanCreated evt, CancellationToken ct)
    {
        var (subject, body) = NotificationTemplates.LoanReceipt(
            new LoanReceiptData("Usuario", "Libro", evt.DueDate));

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
