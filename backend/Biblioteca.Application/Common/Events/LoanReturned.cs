using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Common.Events;

public sealed record LoanReturned(
    Guid LoanId,
    Guid UserId,
    Guid BookId,
    Guid BookCopyId,
    Guid ReturnId,
    ReturnCondition Condition) : IDomainEvent;
