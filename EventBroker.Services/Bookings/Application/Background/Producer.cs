using Bookings.Application.Contracts.Persistence;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Bookings.Application.Background;

internal class Producer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly ILogger<Producer> _logger;

    private const int ITERATION_DELAY_SEC = 2;
    private const string MARKER = "Booking.API [Producer]";
    private const string TRACE_HEADER = "trace-id";
    private const string TRACEPARENT_HEADER = "traceparent";

    public Producer(
        IServiceProvider serviceProvider,
        IProducer<string, string> kafkaProducer,
        ILogger<Producer> logger)
    {
        _serviceProvider = serviceProvider;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Marker}: запуск", MARKER);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("{Marker}: отправка сообщений в Kafka прервана пользователем.", MARKER);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Marker}: критическая ошибка в цикле воркера Outbox.", MARKER);
            }
            await Task.Delay(TimeSpan.FromSeconds(ITERATION_DELAY_SEC), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var outboxMessages = await context.Outbox
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.TimeStampUtc)
            .Take(50)
            .ToListAsync(stoppingToken);

        if (outboxMessages.Count == 0) return;

        try
        {
            foreach (var outboxMessage in outboxMessages)
            {
                try
                {
                    var kafkaMessage = new Message<string, string>
                    {
                        Key = outboxMessage.PartitionKey.ToString(),
                        Value = outboxMessage.Content,
                        Headers = new Headers
                        {
                            { TRACE_HEADER, Encoding.UTF8.GetBytes(outboxMessage.TraceId.ToString()) }
                        }
                    };

                    using var activity = StartPublishActivity(outboxMessage.Topic, outboxMessage.TraceId, kafkaMessage.Headers);

                    var deliveryResult = await _kafkaProducer.ProduceAsync(outboxMessage.Topic, kafkaMessage, stoppingToken);

                    if (deliveryResult.Status == PersistenceStatus.Persisted)
                    {
                        _logger.LogInformation("{MARKER}: сообщение {@Message} отправлено в шину", MARKER, outboxMessage.Content);

                        outboxMessage.ProcessedAtUtc = DateTime.UtcNow;
                        outboxMessage.Error = null;
                    }
                }
                catch (ProduceException<string, string> ex)
                {
                    _logger.LogError(ex, "{Marker}: не удалось отправить сообщение Outbox {Id} в Kafka.", MARKER, outboxMessage.TraceId);

                    outboxMessage.ProcessedAtUtc = DateTime.UtcNow;
                    outboxMessage.Error = $"{MARKER}: Kafka Error: {ex.Error.Reason}";
                }
            }

            await context.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Marker}: общая системная ошибка при обработке пачки.", MARKER);
        }
    }

    /// <summary>
    /// Publish-спан саги + W3C-контекст в хедеры.
    /// trace-id распределённого трейса = бизнес-TraceId (GUID 128 бит совместим с форматом W3C).
    /// Строится из строки outbox'а: в фоновом воркере ambient Activity нет
    /// (бизнес-операция уже завершена), поэтому контекст родителя взять неоткуда — и он не нужен.
    /// </summary>
    private static Activity? StartPublishActivity(string topic, Guid traceId, Headers headers)
    {
        // Контекст саги: "00-{trace-id}-{новый span-id}-01" (sampled).
        var sagaContext = ActivityContext.TryParse(
            $"00-{traceId:N}-{ActivitySpanId.CreateRandom().ToHexString()}-01", null, out var parsed)
            ? parsed
            : default;

        var activity = MessagingActivities.Source.StartActivity($"publish {topic}", ActivityKind.Producer, sagaContext);
        if (activity is null)
            return null;

        activity.SetTag("messaging.destination", topic);
        activity.SetTag("saga.trace_id", traceId.ToString());

        // В хедер едет контекст именно publish-спана: консьюмер станет его ребёнком.
        var context = activity.Context;
        headers.Add(TRACEPARENT_HEADER, Encoding.UTF8.GetBytes(
            $"00-{context.TraceId.ToHexString()}-{context.SpanId.ToHexString()}-{(context.TraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00")}"));

        return activity;
    }

    public override void Dispose()
    {
        _kafkaProducer.Flush(TimeSpan.FromSeconds(10));
        base.Dispose();
    }
}
