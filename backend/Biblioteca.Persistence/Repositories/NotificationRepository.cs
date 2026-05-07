using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class NotificationRepository(BibliotecaDbContext dbContext) : INotificationRepository
{
    public async Task<Notification> AddAsync(Notification notification, CancellationToken ct)
    {
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(ct);
        return notification;
    }

    public async Task<IReadOnlyCollection<Notification>> GetPendingAsync(int limit, CancellationToken ct) =>
        await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.Status == NotificationStatus.PENDING)
            .OrderBy(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task UpdateAsync(Notification notification, CancellationToken ct)
    {
        dbContext.Notifications.Update(notification);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<Notification>> GetByUserAsync(
        Guid userId, bool unreadOnly, CancellationToken ct)
    {
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => n.Status == NotificationStatus.PENDING);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }
}
