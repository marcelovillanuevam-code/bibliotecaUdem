using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class ReservationRepository(BibliotecaDbContext dbContext) : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await dbContext.Reservations
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Reservation?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) =>
        await dbContext.Reservations
            .Include(r => r.User)
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyCollection<Reservation>> GetByUserAsync(
        Guid userId, ReservationStatus? statusFilter, CancellationToken ct)
    {
        var query = dbContext.Reservations
            .AsNoTracking()
            .Include(r => r.User)
                .ThenInclude(u => u!.Profile)
            .Include(r => r.Book)
            .Where(r => r.UserId == userId);

        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        return await query
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByBookAsync(
        Guid bookId, ReservationStatus? statusFilter, CancellationToken ct)
    {
        var query = dbContext.Reservations
            .AsNoTracking()
            .Include(r => r.User)
                .ThenInclude(u => u!.Profile)
            .Include(r => r.Book)
            .Where(r => r.BookId == bookId);

        if (statusFilter.HasValue)
            query = query.Where(r => r.Status == statusFilter.Value);

        return await query
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);
    }

    public async Task<Reservation?> GetActiveByUserAndBookAsync(
        Guid userId, Guid bookId, CancellationToken ct) =>
        await dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.BookId == bookId &&
                (r.Status == ReservationStatus.PENDING || r.Status == ReservationStatus.READY),
                ct);

    public async Task<Reservation?> GetReadyByUserAndBookForUpdateAsync(
        Guid userId, Guid bookId, CancellationToken ct) =>
        await dbContext.Reservations
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.BookId == bookId &&
                r.Status == ReservationStatus.READY,
                ct);

    public async Task<Reservation?> GetNextInQueueAsync(Guid bookId, CancellationToken ct) =>
        await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.BookId == bookId && r.Status == ReservationStatus.PENDING)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyCollection<Reservation>> GetExpiredReadyAsync(CancellationToken ct) =>
        await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.READY && r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<int> GetMaxQueuePositionAsync(Guid bookId, CancellationToken ct)
    {
        var max = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.BookId == bookId &&
                        (r.Status == ReservationStatus.PENDING || r.Status == ReservationStatus.READY))
            .MaxAsync(r => (int?)r.QueuePosition, ct);

        return max ?? 0;
    }

    public async Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct)
    {
        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync(ct);
        return await GetByIdAsync(reservation.Id, ct)
            ?? throw new InvalidOperationException("No se pudo recuperar la reserva recién creada.");
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken ct)
    {
        dbContext.Reservations.Update(reservation);
        await dbContext.SaveChangesAsync(ct);
    }
}
