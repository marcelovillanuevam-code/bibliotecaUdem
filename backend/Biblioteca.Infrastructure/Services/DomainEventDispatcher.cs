using Biblioteca.Application.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Infrastructure.Services;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken ct) where TEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        foreach (var handler in handlers)
            await handler.HandleAsync(domainEvent, ct);
    }
}
