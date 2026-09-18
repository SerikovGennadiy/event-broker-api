using Events.Application.Common.Messaging;
using Events.Application.Contracts.Persistence;
using Events.Application.Contracts.Services.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Events.Application.Services.Messaging;

internal sealed class InboxService(IAppDbContext context) : IInboxService
{
    private readonly int MAX_DELIVERY_ATTEMPTS = 5;
    public async Task<bool> HasBeenProcessedAsync(Guid messageId, string messageType, CancellationToken cancellationToken = default)
    {
        var message = await context.Inbox.AsNoTracking()
                                   .FirstOrDefaultAsync(m => m.TraceId == messageId && m.Type == messageType, cancellationToken);

        return message?.ProcessedAtUtc is not null;
    }

    public async Task<bool> ReceiveAsync(Guid messageId, string messageType, string? content, CancellationToken cancellationToken = default)
    {
        var existingMessage = await context.Inbox.FirstOrDefaultAsync(x => x.TraceId == messageId && x.Type == messageType, cancellationToken);

        if (existingMessage is not null)
        {
            if (existingMessage.ProcessedAtUtc is not null)
                return false;

            if (existingMessage.ReadAttempts > MAX_DELIVERY_ATTEMPTS)
            {
                existingMessage.Content = content;
                existingMessage.Error = $"Нечитаемое сообщение [Poison Pill]: превышен лимит попыток {MAX_DELIVERY_ATTEMPTS}";
                existingMessage.ProcessedAtUtc = DateTime.UtcNow;

                return false;
            }

            existingMessage.ReadAttempts++;
            return true;
        }

        try
        {
            await context.Inbox.AddAsync(new InboxMessage
            {
                TraceId = messageId,
                Type = messageType,
                Content = null,
                ReadAttempts = 1,
                ReceivedAtUtc = DateTime.UtcNow,
            }, cancellationToken);

            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task MarkAsProcessedAsync(Guid messageId, string messageType, CancellationToken cancellationToken = default)
    {
        var message = await GetOrThrowAsync(messageId, messageType, cancellationToken);

        message.ProcessedAtUtc = DateTime.UtcNow;
        message.Error = null;
    }

    public async Task MarkAsFailedAsync(Guid messageId, string messageType, string errorMessage, CancellationToken cancellationToken = default)
    {
        var message = await GetOrThrowAsync(messageId, messageType, cancellationToken);
        message.ProcessedAtUtc = DateTime.UtcNow;
        message.Error = errorMessage;
    }

    private async Task<InboxMessage> GetOrThrowAsync(Guid messageId, string messageType, CancellationToken cancellationToken)
    {
        // Transactinoal Inbox save changes перед коммитом всей транзакции
        // Если мы только что сделали AddAsync на текущем шаге транзакции, Local его мгновенно найдет!
        var localMessage = context.Inbox.Local.FirstOrDefault(m => m.TraceId == messageId && m.Type == messageType);
        if (localMessage is not null)
        {
            return localMessage;
        }

        // Если в памяти нет — идем искать в физическую СУБД (для повторных попыток Redelivery)
        return await context.Inbox.FirstOrDefaultAsync(m => m.TraceId == messageId && m.Type == messageType, cancellationToken)
            ?? throw new InvalidOperationException($"Сообщение {messageId} не найдено в Inbox. Сначала вызовите ReceiveAsync.");
    }
}
