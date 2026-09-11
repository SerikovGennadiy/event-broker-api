using Messaging.Saga;

namespace Events.Application.Contracts.Services.Messaging;
public interface IOutboxService
{
    Task EnqueueMessageAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken) where TEvent : IIntegrationEvent;
}