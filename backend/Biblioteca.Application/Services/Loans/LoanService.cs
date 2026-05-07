using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Common.Services;
using Biblioteca.Application.DTOs.Prestamos;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Options;
using Biblioteca.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Biblioteca.Application.Services.Loans;

public sealed class LoanService(
    ILoanRepository loanRepository,
    IBookCopyRepository bookCopyRepository,
    IUsuarioRepository usuarioRepository,
    IUserEligibilityService eligibilityService,
    IReservationQueryService reservationQueryService,
    IReservationRepository reservationRepository,
    IDomainEventDispatcher dispatcher,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IOptions<LoansOptions> loansOptions,
    ILogger<LoanService> logger) : ILoanService
{
    private const int MaxRenewals = 2;
    private const int RenewalDays = 7;

    public async Task<LoanDto> CreateAsync(CreateLoanRequest request, Guid issuedBy, CancellationToken ct)
    {
        var borrower = await usuarioRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var eligibility = await eligibilityService.CheckAsync(request.UserId, ct);
        if (!eligibility.IsEligible)
            throw new ConflictException(eligibility.Reason ?? "Usuario no elegible para préstamos.");

        var userRole = GetPrimaryRole(borrower);
        var maxActive = loansOptions.Value.GetMaxActive(userRole);
        var activeCount = await loanRepository.CountActiveByUserAsync(request.UserId, ct);
        if (activeCount >= maxActive)
            throw new LoanLimitExceededException();

        var bookCopy = await bookCopyRepository.GetByIdForUpdateAsync(request.BookCopyId, ct)
            ?? throw new NotFoundException("Ejemplar no encontrado.");

        if (bookCopy.Status != BookCopyStatus.Available)
            throw new ConflictException("El ejemplar no está disponible para préstamo.");

        var existing = await loanRepository.GetActiveByBookCopyAsync(request.BookCopyId, ct);
        if (existing is not null)
            throw new ConflictException("El ejemplar ya tiene un préstamo activo.");

        var now = clock.UtcNow;
        var durationDays = request.DurationDaysOverride ?? loansOptions.Value.GetDefaultDurationDays(userRole);

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            BookCopyId = request.BookCopyId,
            LoanedAt = now,
            DueAt = now.AddDays(durationDays),
            Status = LoanStatus.ACTIVE,
            RenewalCount = 0,
            IssuedByUserId = issuedBy
        };

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await loanRepository.AddAsync(loan, ct);

            var readyReservation = await reservationRepository.GetReadyByUserAndBookForUpdateAsync(
                request.UserId,
                bookCopy.BookId,
                ct);

            if (readyReservation is not null)
            {
                readyReservation.Status = ReservationStatus.FULFILLED;
                readyReservation.FulfilledAt = now;
                readyReservation.FulfilledByLoanId = loan.Id;
                await reservationRepository.UpdateAsync(readyReservation, ct);
            }

            bookCopy.Status = BookCopyStatus.Loaned;
            bookCopy.UpdatedAt = now;
            await bookCopyRepository.SaveChangesAsync(ct);

            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }

        try
        {
            await dispatcher.DispatchAsync(new LoanCreated(loan.Id, loan.UserId, bookCopy.BookId, loan.BookCopyId, loan.DueAt), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al despachar LoanCreated para préstamo {LoanId}.", loan.Id);
        }

        return await GetByIdAsync(loan.Id, ct) ?? throw new InvalidOperationException("Error al recuperar el préstamo creado.");
    }

    public async Task<LoanDto> RenewAsync(Guid loanId, Guid requestedBy, CancellationToken ct)
    {
        var loan = await loanRepository.GetByIdForUpdateAsync(loanId, ct)
            ?? throw new NotFoundException("Préstamo no encontrado.");

        if (loan.Status != LoanStatus.ACTIVE)
            throw new ConflictException("Solo se pueden renovar préstamos activos.");

        if (loan.RenewalCount >= MaxRenewals)
            throw new ConflictException("Máximo de renovaciones alcanzado.");

        var bookId = loan.BookCopy?.BookId
            ?? throw new InvalidOperationException("No se pudo determinar el libro del préstamo.");

        var hasReservations = await reservationQueryService.HasActiveReservationsForBookAsync(bookId, ct);
        if (hasReservations)
            throw new ConflictException("No se puede renovar: hay reservas activas para este título.");

        var now = clock.UtcNow;
        var renewal = new LoanRenewal
        {
            Id = Guid.NewGuid(),
            LoanId = loanId,
            RenewedAt = now,
            PreviousDueAt = loan.DueAt,
            NewDueAt = loan.DueAt.AddDays(RenewalDays),
            RenewedByUserId = requestedBy
        };

        loan.DueAt = renewal.NewDueAt;
        loan.RenewalCount++;
        loan.Renewals.Add(renewal);

        await loanRepository.UpdateAsync(loan, ct);

        return await GetByIdAsync(loanId, ct) ?? throw new InvalidOperationException("Error al recuperar el préstamo renovado.");
    }

    public async Task<LoanDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var loan = await loanRepository.GetByIdAsync(id, ct);
        return loan is null ? null : MapToDto(loan);
    }

    public async Task<IReadOnlyCollection<LoanDto>> GetActiveByUserAsync(Guid userId, CancellationToken ct)
    {
        var loans = await loanRepository.GetActiveByUserAsync(userId, ct);
        return loans.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyCollection<LoanDto>> GetByUserAsync(Guid userId, LoanStatus? statusFilter, CancellationToken ct)
    {
        var loans = await loanRepository.GetByUserAsync(userId, statusFilter, ct);
        return loans.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyCollection<LoanDto>> GetAllAsync(LoanStatus? statusFilter, CancellationToken ct, string? copyBarcode = null)
    {
        var loans = await loanRepository.GetAllAsync(statusFilter, ct, copyBarcode);
        return loans.Select(MapToDto).ToList();
    }

    public async Task MarkOverdueAsync(CancellationToken ct)
    {
        var overdueLoans = await loanRepository.GetOverdueActiveAsync(ct);
        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.OVERDUE;
            await loanRepository.UpdateAsync(loan, ct);
        }
    }

    private static string GetPrimaryRole(Usuario user)
    {
        var roles = user.Roles.Select(r => r.Role?.Code ?? string.Empty).ToHashSet();
        if (roles.Contains("ADMIN")) return "ADMIN";
        if (roles.Contains("LIBRARIAN")) return "LIBRARIAN";
        if (roles.Contains("TEACHER")) return "TEACHER";
        return "STUDENT";
    }

    private static LoanDto MapToDto(Loan l) => new(
        l.Id,
        l.UserId,
        UserFullName: l.User?.Profile is { } p ? $"{p.FirstName} {p.LastName}".Trim() : "N/A",
        l.BookCopyId,
        BookTitle: l.BookCopy?.Book?.Title ?? "N/A",
        Isbn: l.BookCopy?.Book?.Isbn ?? "N/A",
        l.LoanedAt,
        l.DueAt,
        l.ReturnedAt,
        Status: l.Status.ToString(),
        l.RenewalCount,
        Renewals: l.Renewals
            .OrderBy(r => r.RenewedAt)
            .Select(r => new LoanRenewalDto(
                r.Id,
                r.LoanId,
                r.RenewedAt,
                r.PreviousDueAt,
                r.NewDueAt,
                r.RenewedByUserId))
            .ToList(),
        CopyBarcode: l.BookCopy?.Barcode ?? "N/A",
        BorrowerName: l.User?.Profile is { } prof ? $"{prof.FirstName} {prof.LastName}".Trim() : "N/A",
        BorrowerEmail: l.User?.Username ?? "N/A");
}
