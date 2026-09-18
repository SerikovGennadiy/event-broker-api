using Confluent.Kafka;
using Events.Application.Contracts.Services;
using Events.Application.Contracts.Services.Messaging;
using Events.Domain.Exceptions;
using Messaging.Bookings;
using Messaging.Events;
using Messaging.Saga;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IAppDbContext = Events.Application.Contracts.Persistence.IAppDbContext;

namespace Events.Application.Background;

internal class Consumer(IServiceProvider provider, ILogger<Consumer> logger) : BackgroundService
{
    private const int BAD_READING_PAUSE_SEC = 2;
    private const string MARKER = "Event.API [Consumer]";
    private const string TRACE_HEADER = "trace-id";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private static readonly string[] TopicsList = [
        Messaging.Topics.BookingProcessing,
        Messaging.Topics.EventIntegration
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
     
        logger.LogInformation("{Marker} : запуск...", MARKER);
        using var consumer = provider.GetRequiredService<IConsumer<string, string>>();

        try
        {
            consumer.Subscribe(TopicsList);
            logger.LogInformation("{Marker} : подключение выполнено, консьюмер слушает топики.", MARKER);

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "{Marker} : ошибка чтения из Kafka: {Reason}", MARKER, ex.Error.Reason);
                    await Task.Delay(TimeSpan.FromSeconds(BAD_READING_PAUSE_SEC), stoppingToken);
                    continue;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("{Marker} : получен сигнал остановки процесса чтения.", MARKER);
                    break;
                }

                try
                {
                    await ProcessMessageAsync(result, stoppingToken);

                    consumer.Commit(result);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "{Marker} : системная ошибка, повтор 3 секунды...", MARKER);

                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                }
            }
        }
        finally
        {
            consumer.Close();
            logger.LogInformation("{Marker} : отключение выполнено, ребалансировка", MARKER);
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken stoppingToken)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();

        var messageId = ExtraceMessageId(result);
        var messageType = ExtractMessageType(result);

        if (await inboxService.HasBeenProcessedAsync(messageId, messageType, stoppingToken))
            return;

        using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
        try
        {
            bool isReceivedAndReadyForBusiness = await inboxService.ReceiveAsync(messageId, messageType, result.Message.Value, stoppingToken);
            if (isReceivedAndReadyForBusiness)
            {
                try
                {
                    await (result.Topic switch
                    {
                        Messaging.Topics.BookingProcessing => HandleBookingProcessingAsync(result, scope, stoppingToken),
                        _ => throw new InvalidOperationException($"{MARKER}: Сервис не обрабатывает топик: {result.Topic}")
                    });

                    await inboxService.MarkAsProcessedAsync(messageId, messageType, stoppingToken);
                }
                catch (Exception ex) when (ex is JsonException or InvalidOperationException or DomainException)
                {
                    await inboxService.MarkAsFailedAsync(messageId, messageType, ex.Message, stoppingToken);
                    logger.LogWarning(ex, "{Marker} : Бизнес-обработка сообщения {Id} завершилась ошибкой, сбой зафиксирован в Inbox.", MARKER, messageId);
                }
            }

            await context.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(stoppingToken);
            throw;
        }
    }

    private async Task HandleBookingProcessingAsync(ConsumeResult<string, string> result, IServiceScope scope, CancellationToken stoppingToken)
    {
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var @event = JsonSerializer.Deserialize<IIntegrationMessage>(result.Message.Value, Options);

        switch(@event)
        {
            case BookingStarted booking:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(BookingStarted), booking);
                await eventService.ReserveSeats(traceId: booking.TraceId,
                                                eventId: booking.EventId,
                                                bookingId: booking.BookingId,
                                                userId: booking.UserId,
                                                cancellationToken: stoppingToken);
                break;
            case BookingRejected booking:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(BookingRejected), booking);
                await eventService.ReleaseSeats(traceId: booking.TraceId,
                                eventId: booking.EventId,
                                bookingId: booking.BookingId,
                                userId: booking.UserId,
                                cancellationToken: stoppingToken);
                break;
            case BookingCancelled booking:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(BookingCancelled), booking);
                await eventService.ReleaseSeats(traceId: booking.TraceId,
                                eventId: booking.EventId,
                                bookingId: booking.BookingId,
                                userId: booking.UserId,
                                cancellationToken: stoppingToken);
                break;

        }
    }

    private static Guid ExtraceMessageId(ConsumeResult<string, string> result)
    {
        var header = result.Message.Headers.FirstOrDefault(x => x.Key == TRACE_HEADER);
        if (header is not null && Guid.TryParse(Encoding.UTF8.GetString(header.GetValueBytes()), out Guid traceId))
            return traceId;

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{result.Topic}|{result.Message.Key}|{result.Message.Value}"));
        return new Guid(hash[..16]);
    }

    private static string ExtractMessageType(ConsumeResult<string, string> result)
    {
        var message = result.Message.Value ?? string.Empty;
        var doc = JsonDocument.Parse(message);

        return doc.RootElement.TryGetProperty("$type", out var messageTypeDiscriminator) ?
            messageTypeDiscriminator.ToString() :
            throw new JsonException();
    }
}
