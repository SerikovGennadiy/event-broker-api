using Messaging.Saga;

namespace Bookings.Application.Contracts.Messaging;
public interface IOutboxService
{
    Task EnqueueMessageAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken) where TEvent : IIntegarationEvent;
}