namespace Biblioteca.Application.Common.Events;

public sealed record LoanCreated(
    Guid LoanId,
    Guid UserId,
    Guid BookId,
    Guid BookCopyId,
    DateTime DueDate) : IDomainEvent;
