using Messaging.Saga;
using System.Text.Json;
using Bookings.Application.Common.Messaging;
using Bookings.Application.Contracts.Messaging;
using Bookings.Application.Contracts.Persistence;

namespace Bookings.Application.Services.Messaging;

public class OutboxService(IAppDbContext context) : IOutboxService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public async Task EnqueueMessageAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken) where TEvent : IIntegrationMessage
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            TraceId = @event.TraceId,
            Type = @event.GetType().FullName ?? throw new InvalidOperationException("Тип сообщения не определен"),
            Content = JsonSerializer.Serialize<IIntegrationMessage>(@event, Options),
            Topic = topic,
            PartitionKey = @event.PartitionKey,
            TimeStampUtc = DateTime.UtcNow,
        };

        await context.Outbox.AddAsync(outboxMessage, cancellationToken);
    }
}
