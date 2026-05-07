using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Loans.EventHandlers;

public sealed class LoanReturnedHandler(
    ILoanRepository loanRepository,
    IBookCopyRepository bookCopyRepository,
    IDateTimeProvider clock) : IDomainEventHandler<LoanReturned>
{
    public async Task HandleAsync(LoanReturned evt, CancellationToken ct)
    {
        var loan = await loanRepository.GetByIdForUpdateAsync(evt.LoanId, ct);
        if (loan is null) return;

        var now = clock.UtcNow;
        loan.ReturnedAt = now;
        loan.Status = evt.Condition == ReturnCondition.LOST ? LoanStatus.LOST : LoanStatus.RETURNED;

        await loanRepository.UpdateAsync(loan, ct);

        var bookCopy = await bookCopyRepository.GetByIdForUpdateAsync(evt.BookCopyId, ct);
        if (bookCopy is null) return;

        bookCopy.UpdatedAt = now;
        bookCopy.Status = evt.Condition == ReturnCondition.LOST
            ? BookCopyStatus.Lost
            : BookCopyStatus.Available;

        await bookCopyRepository.SaveChangesAsync(ct);
    }
}
