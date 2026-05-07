using System.Text.Json;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biblioteca.Infrastructure.Services;

public sealed class ReservationExpirationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservationExpirationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ReadyWindow = TimeSpan.FromHours(48);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reservationRepo = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

                var expired = await reservationRepo.GetExpiredReadyAsync(stoppingToken);
                var now = clock.UtcNow;

                foreach (var reservation in expired)
                {
                    reservation.Status = ReservationStatus.EXPIRED;
                    await reservationRepo.UpdateAsync(reservation, stoppingToken);

                    var next = await reservationRepo.GetNextInQueueAsync(reservation.BookId, stoppingToken);
                    if (next is not null)
                    {
                        next.Status = ReservationStatus.READY;
                        next.ReadyAt = now;
                        next.ExpiresAt = now.Add(ReadyWindow);
                        await reservationRepo.UpdateAsync(next, stoppingToken);

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
                            PayloadJson = JsonSerializer.Serialize(new { reservationId = next.Id, bookId = next.BookId })
                        };
                        await notificationRepo.AddAsync(notification, stoppingToken);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error al expirar reservas.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
