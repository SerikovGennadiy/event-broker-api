using Bookings.Application.Common.Messaging;
using Bookings.Application.Contracts.Messaging;
using Bookings.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Application.Services.Messaging;

public class InboxService(IAppDbContext context) : IInboxService
{
    private readonly int MAX_DELIVERY_ATTEMPTS = 5;
    public async Task<bool> HasBeenProcessedAsync(Guid messageId, string messageType, CancellationToken cancellationToken = default)
    {
        var message = await context.Inbox.AsNoTracking()
                                   .FirstOrDefaultAsync(m => m.TraceId == messageId && m.Type == messageType, cancellationToken);

        return message?.ProcessedAtUtc is not null;
    }

    public async Task<bool> ReceiveAsync(Guid messageId, string messageType, string content, CancellationToken cancellationToken = default)
    {
        var existingMessage = await context.Inbox.FirstOrDefaultAsync(x => x.TraceId == messageId && x.Type == messageType, cancellationToken);

        if (existingMessage is not null)
        {
            if (existingMessage.ProcessedAtUtc is not null)
                return false; // уже обработано - чистый дубль

            if (existingMessage.ReadAttempts > MAX_DELIVERY_ATTEMPTS)
            {
                existingMessage.Content = content ?? "y";
                existingMessage.Error = $"Нечитаемое сообщение [Poison Pill]: превышен лимит попыток {MAX_DELIVERY_ATTEMPTS}";
                existingMessage.ProcessedAtUtc = DateTime.UtcNow; // Искусственно закрываем шаг, чтобы сдвинуть офсет в Kafka

                return false; // консьюмер пропустит вызов бизнес-логики, но закоммитит inbox СУБД и offset в Kafka
            }

            existingMessage.ReadAttempts++; // чтение выполнено, ProcessedAt == null -> проброс в бизнес-логику
            return true;
        }

        try
        {
            await context.Inbox.AddAsync(new InboxMessage
            {
                TraceId = messageId,
                Type = messageType,
                Content = null, // в при нормально работе в БД ничего пишем
                ReadAttempts = 1, // рабочая попытка чтения
                ReceivedAtUtc = DateTime.UtcNow,
            }, cancellationToken);

            return true;
        }
        catch (DbUpdateException)
        {
            // защита от параллельной гонки нескольких экземпляров сервиса в одну БД
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
