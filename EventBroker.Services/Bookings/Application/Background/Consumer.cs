using Bookings.Application.Contracts.Messaging;
using Bookings.Application.Contracts.Persistence;
using Bookings.Application.Contracts.Services;
using Bookings.Domain.Exceptions;
using Bookings.Domain.Models;
using Confluent.Kafka;
using Messaging.Events;
using Messaging.Saga;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bookings.Application.Background;

internal sealed class Consumer(IServiceProvider provider, ILogger<Consumer> logger) : BackgroundService
{
    private const int BAD_READING_PAUSE_SEC = 2;
    private const string MARKER = "Booking.API:[Consumer]";
    private const string TRACE_HEADER = "trace-id";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private static readonly string[] TopicsList = [
        Messaging.Topics.BookingProcessing,
        Messaging.Topics.EventIntegration,
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

        var messageId = ExtractTraceId(result);
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
                        Messaging.Topics.EventIntegration => HandleEventIntegrationAsync(result, scope, stoppingToken),
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
            return;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(stoppingToken);
            throw;
        }
    }

    private async Task HandleEventIntegrationAsync(ConsumeResult<string, string> result, IServiceScope scope, CancellationToken stoppingToken)
    {
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var repo = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();

        var @event = JsonSerializer.Deserialize<IIntegrationMessage>(result.Message.Value, Options);

        switch (@event)
        {
            case EventCreatedOrUpdated m:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(EventCreatedOrUpdated), m);
                await repo.EventRead.AddAsync(new Common.DTO.EventReadDTO(m.EventId, m.StartAt), stoppingToken);
                break;

            case EventDeleted m:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(EventDeleted), m);
                await bookingService.RemoveByEventAsync(m.EventId, stoppingToken);
                await repo.EventRead.DeleteAsync(m.EventId, stoppingToken);
                break;
            default:
                logger.LogWarning("{Marker}: пока собственный outbox попадается в собственный inbox {@event}", MARKER, @event);
                break;
        }
    }

    private async Task HandleBookingProcessingAsync(ConsumeResult<string, string> result, IServiceScope scope, CancellationToken stoppingToken)
    {
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var @event = JsonSerializer.Deserialize<IIntegrationMessage>(result.Message.Value, Options);

        switch (@event)
        {
            case SeatReserved booking:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(SeatReserved), booking);
                await bookingService.ConfirmBookingAsync(booking.TraceId, booking.BookingId, booking.EventId, booking.UserId, stoppingToken);
                break;
            case SeatReservationFailed failed:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(SeatReservationFailed), failed);
                await bookingService.RejectBooingAsync(traceId: failed.TraceId, bookingId: failed.BookingId, eventId: failed.EventId, userId: failed.UserId, reason: failed.Error,  stoppingToken);
                break;
            case SeatReleased released:
                logger.LogInformation("{Marker}: Получено сообщение {MessageType}: {@Message}", MARKER, nameof(SeatReleased), released);
                await bookingService.RejectBooingAsync(traceId: released.TraceId, bookingId: released.BookingId, eventId: released.EventId, userId: released.UserId, reason: "Места освобождены по инициативе пользователя", stoppingToken);
                break;
            default:
                logger.LogWarning("{Marker}: пока собственный outbщ попадается в собственный inbox {@event}", MARKER, @event);
                break;

        }
    }

    private static Guid ExtractTraceId(ConsumeResult<string, string> result)
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