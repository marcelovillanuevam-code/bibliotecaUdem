using Biblioteca.Application.Interfaces.Reportes;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class ReportesRepository(BibliotecaDbContext db) : IReportesRepository
{
    public async Task<IReadOnlyCollection<Fine>> GetPaidFinesAsync(DateTime from, DateTime to, CancellationToken ct) =>
        await db.Fines
            .AsNoTracking()
            .Include(f => f.User!).ThenInclude(u => u.Profile)
            .Include(f => f.User!).ThenInclude(u => u.Contacts)
            .Where(f => f.Status == FineStatus.PAID && f.PaidAt >= from && f.PaidAt < to)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<Fine>> GetPendingFinesAsync(CancellationToken ct) =>
        await db.Fines
            .AsNoTracking()
            .Include(f => f.User!).ThenInclude(u => u.Profile)
            .Include(f => f.User!).ThenInclude(u => u.Contacts)
            .Where(f => f.Status == FineStatus.PENDING)
            .ToListAsync(ct);

    public async Task<(int TotalDevoluciones, IReadOnlyCollection<int?> DiasRetraso)> GetDevolucionesTardiasDataAsync(
        DateTime from, DateTime to, CancellationToken ct)
    {
        var totalDevoluciones = await db.Returns
            .AsNoTracking()
            .CountAsync(r => r.ReturnedAt >= from && r.ReturnedAt < to, ct);

        var diasRetraso = await db.Fines
            .AsNoTracking()
            .Where(f => f.Reason == FineReason.LATE
                     && f.Return!.ReturnedAt >= from
                     && f.Return!.ReturnedAt < to)
            .Select(f => f.DaysLate)
            .ToListAsync(ct);

        return (totalDevoluciones, diasRetraso);
    }

    public async Task<IReadOnlyCollection<Fine>> GetWaivedFinesAsync(DateTime from, DateTime to, CancellationToken ct) =>
        await db.Fines
            .AsNoTracking()
            .Include(f => f.User!).ThenInclude(u => u.Profile)
            .Include(f => f.User!).ThenInclude(u => u.Contacts)
            .Where(f => f.Status == FineStatus.WAIVED && f.PaidAt >= from && f.PaidAt < to)
            .ToListAsync(ct);

    public async Task<Dictionary<Guid, Usuario>> GetUsuariosByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
        await db.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .Include(u => u.Contacts)
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);
}
