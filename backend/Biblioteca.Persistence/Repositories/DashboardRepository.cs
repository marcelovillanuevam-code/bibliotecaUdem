using Biblioteca.Application.DTOs.Dashboard;
using Biblioteca.Application.Interfaces.Dashboard;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Persistence.Repositories;

public sealed class DashboardRepository(IDbContextFactory<BibliotecaDbContext> dbContextFactory)
    : IDashboardRepository
{
    public async Task<BooksKpisDto> GetBooksAsync(CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var result = await db.Books
            .IgnoreQueryFilters()
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(b => b.DeletedAt == null)
            })
            .SingleOrDefaultAsync(ct);

        return new BooksKpisDto(result?.Total ?? 0, result?.Active ?? 0);
    }

    public async Task<CopiesKpisDto> GetCopiesAsync(CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var counts = await db.BookCopies
            .AsNoTracking()
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Total = g.Count() })
            .ToListAsync(ct);

        var byStatus = counts.ToDictionary(c => c.Status, c => c.Total, StringComparer.OrdinalIgnoreCase);
        var total = counts.Sum(c => c.Total);

        // Count non-deleted copies that actually have an active/overdue loan.
        // This is source-of-truth for "in use" and is robust against Status/loan sync gaps
        // (e.g. a copy stuck as LOANED after its return was processed).
        var copiesOnActiveLoan = await db.BookCopies
            .AsNoTracking()
            .Where(bc => db.Loans.Any(l => l.BookCopyId == bc.Id &&
                                           (l.Status == LoanStatus.ACTIVE || l.Status == LoanStatus.OVERDUE)))
            .CountAsync(ct);

        // Copies in permanently non-lendable states (unrelated to current loans)
        var notLendable = byStatus.GetValueOrDefault(BookCopyStatus.Maintenance)
            + byStatus.GetValueOrDefault(BookCopyStatus.Lost)
            + byStatus.GetValueOrDefault(BookCopyStatus.Retired)
            + byStatus.GetValueOrDefault(BookCopyStatus.Reserved);

        return new CopiesKpisDto(
            total,
            Math.Max(0, total - copiesOnActiveLoan - notLendable),
            byStatus.GetValueOrDefault(BookCopyStatus.Loaned),
            byStatus.GetValueOrDefault(BookCopyStatus.Maintenance));
    }

    public async Task<UsersKpisDto> GetUsersAsync(CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var result = await db.Users
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(u => u.StatusCode == "active")
            })
            .SingleOrDefaultAsync(ct);

        return new UsersKpisDto(result?.Total ?? 0, result?.Active ?? 0);
    }

    public async Task<LoansKpisDto> GetLoansAsync(
        DateTime now,
        DateTime monthStart,
        DateTime nextMonthStart,
        CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var result = await db.Loans
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Active = g.Count(l => l.Status == LoanStatus.ACTIVE),
                Overdue = g.Count(l => l.Status == LoanStatus.OVERDUE ||
                                       (l.Status == LoanStatus.ACTIVE && l.DueAt < now)),
                TotalThisMonth = g.Count(l => l.LoanedAt >= monthStart && l.LoanedAt < nextMonthStart)
            })
            .SingleOrDefaultAsync(ct);

        return new LoansKpisDto(
            result?.Active ?? 0,
            result?.Overdue ?? 0,
            result?.TotalThisMonth ?? 0,
            []);
    }

    public async Task<IReadOnlyCollection<LoanDailyKpiDto>> GetLoanDailyCountsAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var counts = await db.Loans
            .AsNoTracking()
            .Where(l => l.LoanedAt >= from && l.LoanedAt < to)
            .GroupBy(l => l.LoanedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        return counts
            .Select(c => new LoanDailyKpiDto(DateOnly.FromDateTime(c.Date), c.Total))
            .ToList();
    }

    public async Task<FinesKpisDto> GetFinesAsync(
        DateTime monthStart,
        DateTime nextMonthStart,
        CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var result = await db.Fines
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Pending = g.Count(f => f.Status == FineStatus.PENDING),
                TotalAmountPendingMxn = g
                    .Where(f => f.Status == FineStatus.PENDING)
                    .Sum(f => (decimal?)f.Amount) ?? 0m,
                PaidThisMonth = g.Count(f =>
                    f.Status == FineStatus.PAID &&
                    f.PaidAt >= monthStart &&
                    f.PaidAt < nextMonthStart)
            })
            .SingleOrDefaultAsync(ct);

        return new FinesKpisDto(
            result?.Pending ?? 0,
            result?.TotalAmountPendingMxn ?? 0m,
            result?.PaidThisMonth ?? 0);
    }

    public async Task<ReservationsKpisDto> GetReservationsAsync(CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var result = await db.Reservations
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Active = g.Count(r => r.Status == ReservationStatus.PENDING ||
                                      r.Status == ReservationStatus.READY),
                Ready = g.Count(r => r.Status == ReservationStatus.READY)
            })
            .SingleOrDefaultAsync(ct);

        return new ReservationsKpisDto(result?.Active ?? 0, result?.Ready ?? 0);
    }

    public async Task<IReadOnlyCollection<RecentActivityDto>> GetRecentActivityAsync(CancellationToken ct)
    {
        await using var db = dbContextFactory.CreateDbContext();

        var activity = await db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.PerformedAt)
            .Take(10)
            .Select(a => new
            {
                a.Id,
                a.TableName,
                a.Action,
                a.RecordId,
                a.PerformedBy,
                a.PerformedAt
            })
            .ToListAsync(ct);

        return activity
            .Select(a => new RecentActivityDto(
                a.Id,
                a.TableName,
                a.Action,
                a.RecordId,
                a.PerformedBy,
                a.PerformedAt,
                BuildSummary(a.TableName, a.Action)))
            .ToList();
    }

    private static string BuildSummary(string tableName, string action)
    {
        var entity = tableName switch
        {
            "books" => "libro",
            "book_copies" => "ejemplar",
            "loans" => "prestamo",
            "returns" => "devolucion",
            "fines" => "multa",
            "reservations" => "reserva",
            "users" => "usuario",
            _ => tableName
        };

        var verb = action switch
        {
            "INSERT" => "Creacion",
            "UPDATE" => "Actualizacion",
            "DELETE" => "Eliminacion",
            _ => action
        };

        return $"{verb} de {entity}";
    }
}
