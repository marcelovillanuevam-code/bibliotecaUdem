namespace Biblioteca.Application.Common.Events;

public sealed record FinePaid(
    Guid FineId,
    Guid UserId,
    Guid PaidByUserId,
    decimal Amount) : IDomainEvent;
