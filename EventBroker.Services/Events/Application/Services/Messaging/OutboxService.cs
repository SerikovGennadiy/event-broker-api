using Events.Application.Common.Messaging;
using Events.Application.Contracts.Persistence;
using Events.Application.Contracts.Services.Messaging;
using Messaging.Saga;
using System.Text.Json;

namespace Events.Application.Services.Messaging;

public class OutboxService(IAppDbContext context) : IOutboxService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public async Task EnqueueMessageAsync<IEvent>(IEvent @event, string topic, CancellationToken cancellationToken) where IEvent : IIntegrationEvent
    {
        var outputMessage = new OutboxMessage
        {
            TraceId = Guid.CreateVersion7(),
            Type = @event.GetType().FullName ?? throw new InvalidOperationException("Тип сообщения не определен"),
            PartitionKey = @event.PartitionKey,
            Content = JsonSerializer.Serialize<object>(@event, Options),
            Topic = topic,
            TimeStampAt = DateTime.Now,
            ProcessedAtUtc = null,
        };

        await context.Outbox.AddAsync(outputMessage);
    }
}
