using Biblioteca.Application.Services.Dashboard;
using Biblioteca.Domain.Entities;
using Biblioteca.Persistence.Context;
using Biblioteca.Persistence.Repositories;
using Biblioteca.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biblioteca.Tests.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetKpisAsync_devuelve_conteos_reales_con_datos_seed()
    {
        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new BibliotecaDbContext(options, new NullCurrentUserService());
        var dbFactory = new DashboardTestDbContextFactory(options);
        var now = FixedDateTimeProvider.Fixed;

        var activeBook = TestData.NewLibro("Libro activo");
        var deletedBook = TestData.NewLibro("Libro eliminado");
        deletedBook.DeletedAt = now;
        db.Books.AddRange(activeBook, deletedBook);

        var availableCopy = NewCopy(activeBook.Id, BookCopyStatus.Available, "DASH-001");
        var loanedCopy = NewCopy(activeBook.Id, BookCopyStatus.Loaned, "DASH-002");
        var maintenanceCopy = NewCopy(activeBook.Id, BookCopyStatus.Maintenance, "DASH-003");
        var lostCopy = NewCopy(activeBook.Id, BookCopyStatus.Lost, "DASH-004");
        db.BookCopies.AddRange(availableCopy, loanedCopy, maintenanceCopy, lostCopy);

        var activeUser = TestData.NewUsuario("active.user");
        activeUser.StatusCode = "active";
        var pendingUser = TestData.NewUsuario("pending.user");
        pendingUser.StatusCode = "pending_verification";
        var deletedUser = TestData.NewUsuario("deleted.user");
        deletedUser.StatusCode = "active";
        deletedUser.DeletedAt = now;
        db.Users.AddRange(activeUser, pendingUser, deletedUser);

        db.Loans.AddRange(
            NewLoan(activeUser.Id, loanedCopy.Id, LoanStatus.ACTIVE, now, now.AddDays(7)),
            NewLoan(activeUser.Id, loanedCopy.Id, LoanStatus.ACTIVE, now.AddDays(-2), now.AddDays(-1)),
            NewLoan(activeUser.Id, loanedCopy.Id, LoanStatus.OVERDUE, now.AddDays(-3), now.AddDays(-2)),
            NewLoan(activeUser.Id, loanedCopy.Id, LoanStatus.RETURNED, now.AddDays(-6), now.AddDays(-1)),
            NewLoan(activeUser.Id, loanedCopy.Id, LoanStatus.RETURNED, now.AddMonths(-2), now.AddMonths(-2).AddDays(7)));

        db.Fines.AddRange(
            NewFine(activeUser.Id, FineStatus.PENDING, 100m, null),
            NewFine(activeUser.Id, FineStatus.PENDING, 50m, null),
            NewFine(activeUser.Id, FineStatus.PAID, 25m, now.AddDays(-1)),
            NewFine(activeUser.Id, FineStatus.PAID, 40m, now.AddMonths(-1)));

        db.Reservations.AddRange(
            NewReservation(activeUser.Id, activeBook.Id, ReservationStatus.PENDING),
            NewReservation(activeUser.Id, activeBook.Id, ReservationStatus.READY),
            NewReservation(activeUser.Id, activeBook.Id, ReservationStatus.FULFILLED));

        await db.SaveChangesAsync();

        db.AuditLogs.RemoveRange(db.AuditLogs);
        await db.SaveChangesAsync();

        var newestActivityId = Guid.NewGuid();
        for (var i = 0; i < 11; i++)
        {
            db.AuditLogs.Add(new RegistroAuditoria
            {
                Id = i == 10 ? newestActivityId : Guid.NewGuid(),
                TableName = i % 2 == 0 ? "loans" : "fines",
                Action = i % 2 == 0 ? "INSERT" : "UPDATE",
                RecordId = Guid.NewGuid(),
                PerformedAt = now.AddMinutes(i)
            });
        }

        await db.SaveChangesAsync();

        var sut = new DashboardService(
            new DashboardRepository(dbFactory),
            new FixedDateTimeProvider());

        var result = await sut.GetKpisAsync(CancellationToken.None);

        result.Books.Total.Should().Be(2);
        result.Books.Active.Should().Be(1);
        result.Copies.Total.Should().Be(4);
        result.Copies.Available.Should().Be(1);
        result.Copies.Loaned.Should().Be(1);
        result.Copies.Maintenance.Should().Be(1);
        result.Users.Total.Should().Be(2);
        result.Users.Active.Should().Be(1);
        result.Loans.Active.Should().Be(2);
        result.Loans.Overdue.Should().Be(2);
        result.Loans.TotalThisMonth.Should().Be(4);
        result.Loans.Last30Days.Should().HaveCount(30);
        result.Loans.Last30Days.Single(d => d.Date == DateOnly.FromDateTime(now)).Total.Should().Be(1);
        result.Fines.Pending.Should().Be(2);
        result.Fines.TotalAmountPendingMxn.Should().Be(150m);
        result.Fines.PaidThisMonth.Should().Be(1);
        result.Reservations.Active.Should().Be(2);
        result.Reservations.Ready.Should().Be(1);
        result.RecentActivity.Should().HaveCount(10);
        result.RecentActivity.First().Id.Should().Be(newestActivityId);
    }

    private static BookCopy NewCopy(Guid bookId, string status, string barcode) => new()
    {
        Id = Guid.NewGuid(),
        BookId = bookId,
        Barcode = barcode,
        Status = status,
        AcquiredAt = FixedDateTimeProvider.Fixed,
        CreatedAt = FixedDateTimeProvider.Fixed,
        UpdatedAt = FixedDateTimeProvider.Fixed
    };

    private static Loan NewLoan(
        Guid userId,
        Guid bookCopyId,
        LoanStatus status,
        DateTime loanedAt,
        DateTime dueAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        BookCopyId = bookCopyId,
        LoanedAt = loanedAt,
        DueAt = dueAt,
        ReturnedAt = status == LoanStatus.RETURNED ? loanedAt.AddDays(1) : null,
        Status = status,
        IssuedByUserId = userId
    };

    private static Fine NewFine(Guid userId, FineStatus status, decimal amount, DateTime? paidAt) => new()
    {
        Id = Guid.NewGuid(),
        ReturnId = Guid.NewGuid(),
        UserId = userId,
        Reason = FineReason.LATE,
        Amount = amount,
        Status = status,
        CreatedAt = FixedDateTimeProvider.Fixed,
        PaidAt = paidAt
    };

    private static Reservation NewReservation(Guid userId, Guid bookId, ReservationStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        BookId = bookId,
        QueuePosition = 1,
        Status = status,
        CreatedAt = FixedDateTimeProvider.Fixed
    };

    private sealed class DashboardTestDbContextFactory(DbContextOptions<BibliotecaDbContext> options)
        : IDbContextFactory<BibliotecaDbContext>
    {
        public BibliotecaDbContext CreateDbContext() =>
            new(options, new NullCurrentUserService());
    }
}
