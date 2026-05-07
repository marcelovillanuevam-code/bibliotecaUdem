namespace Biblioteca.Application.Common.Events;

public sealed record FineCreated(
    Guid FineId,
    Guid UserId,
    Guid ReturnId,
    string Reason,
    decimal Amount) : IDomainEvent;
