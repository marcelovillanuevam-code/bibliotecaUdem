using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Interfaces.Reservations;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification, CancellationToken ct);
    Task<IReadOnlyCollection<Notification>> GetPendingAsync(int limit, CancellationToken ct);
    Task UpdateAsync(Notification notification, CancellationToken ct);
    Task<IReadOnlyCollection<Notification>> GetByUserAsync(Guid userId, bool unreadOnly, CancellationToken ct);
}
