using Biblioteca.Application.Common.Events;
using Biblioteca.Application.DTOs.Returns;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Application.Services.Fines;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Returns;

public sealed class ReturnService(
    ILoanRepository loanRepository,
    IReturnRepository returnRepository,
    IFineRepository fineRepository,
    FineCalculator fineCalculator,
    IDomainEventDispatcher dispatcher,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IReturnService
{
    public async Task<List<ReturnDto>> ListAsync(CancellationToken ct)
    {
        var returns = await returnRepository.ListAsync(ct);
        return returns.Select(r => MapToDto(r, r.Fine is not null ? [r.Fine] : [])).ToList();
    }

    public async Task<ReturnDto> CreateAsync(CreateReturnRequest request, Guid receivedBy, CancellationToken ct)
    {
        var loan = await loanRepository.GetByIdForUpdateAsync(request.LoanId, ct)
            ?? throw new NotFoundException("Préstamo no encontrado.");

        if (loan.Status != LoanStatus.ACTIVE && loan.Status != LoanStatus.OVERDUE)
            throw new ConflictException("Solo se pueden devolver préstamos activos o vencidos.");

        var existing = await returnRepository.GetByLoanAsync(request.LoanId, ct);
        if (existing is not null)
            throw new ConflictException("Este préstamo ya fue devuelto.");

        var now = clock.UtcNow;
        var returnEntity = new Return
        {
            Id = Guid.NewGuid(),
            LoanId = request.LoanId,
            ReturnedAt = now,
            Condition = request.Condition,
            InspectionNotes = request.InspectionNotes,
            ReceivedByUserId = receivedBy
        };

        var fineDrafts = await fineCalculator.CalculateAsync(loan, request.Condition, now, ct);
        var createdFines = new List<Fine>();

        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await returnRepository.AddAsync(returnEntity, ct);

            foreach (var draft in fineDrafts)
            {
                var fine = new Fine
                {
                    Id = Guid.NewGuid(),
                    ReturnId = returnEntity.Id,
                    UserId = loan.UserId,
                    Reason = draft.Reason,
                    Amount = draft.Amount,
                    DaysLate = draft.DaysLate,
                    Status = FineStatus.PENDING,
                    CreatedAt = now
                };
                await fineRepository.AddAsync(fine, ct);
                createdFines.Add(fine);
            }

            var bookId = loan.BookCopy?.BookId ?? Guid.Empty;
            await dispatcher.DispatchAsync(
                new LoanReturned(loan.Id, loan.UserId, bookId, loan.BookCopyId, returnEntity.Id, request.Condition), ct);

            foreach (var fine in createdFines)
                await dispatcher.DispatchAsync(new FineCreated(fine.Id, fine.UserId, fine.ReturnId, fine.Reason.ToString(), fine.Amount), ct);

            await unitOfWork.CommitAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }

        var saved = await returnRepository.GetByIdAsync(returnEntity.Id, ct)
            ?? throw new InvalidOperationException("Error al recuperar la devolución registrada.");

        return MapToDto(saved, createdFines);
    }

    public async Task<ReturnDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var r = await returnRepository.GetByIdAsync(id, ct);
        if (r is null) return null;

        var fines = r.Fine is not null ? [r.Fine] : Array.Empty<Fine>();
        return MapToDto(r, fines);
    }

    private static ReturnDto MapToDto(Return r, IEnumerable<Fine> fines) => new(
        r.Id,
        r.LoanId,
        r.ReturnedAt,
        r.Condition.ToString(),
        r.InspectionNotes,
        r.ReceivedByUserId,
        r.Loan?.BookCopy?.Book?.Title,
        r.Loan?.BookCopy?.Barcode,
        BorrowerName(r.Loan?.User),
        BorrowerEmail(r.Loan?.User),
        fines.Select(f => new FineDto(f.Id, f.ReturnId, f.UserId, f.Reason.ToString(), f.Amount, f.DaysLate, f.Status.ToString(), f.CreatedAt, f.PaidAt, f.PaidByUserId)).ToList());

    private static string? BorrowerName(Usuario? user)
    {
        if (user is null)
            return null;

        if (user.Profile is not { } profile)
            return user.Username;

        return string.IsNullOrWhiteSpace(profile.DisplayName)
            ? $"{profile.FirstName} {profile.LastName}".Trim()
            : profile.DisplayName;
    }

    private static string? BorrowerEmail(Usuario? user) =>
        user?.Contacts
            .Where(contact => contact.Type.Equals("email", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(contact => contact.IsPrimary)
            .Select(contact => contact.Value)
            .FirstOrDefault();
}
