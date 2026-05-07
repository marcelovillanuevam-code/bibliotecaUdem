namespace Biblioteca.Application.Common.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken ct) where TEvent : IDomainEvent;
}
