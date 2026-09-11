using Confluent.Kafka;
using Events.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Events.Application.Background;

internal sealed class Producer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly ILogger<Producer> _logger;

    private const int ITERATION_DELAY_SEC = 2;
    private const string MARKER = "Event.API [Producer]";

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
            .OrderBy(x => x.TimeStampAt)
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
                            { "trace-id", Encoding.UTF8.GetBytes(outboxMessage.TraceId.ToString()) }
                        }
                    };

                    var deliveryResult = await _kafkaProducer.ProduceAsync(outboxMessage.Topic, kafkaMessage, stoppingToken);

                    if (deliveryResult.Status == PersistenceStatus.Persisted)
                    {
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

    public override void Dispose()
    {
        _kafkaProducer.Flush(TimeSpan.FromSeconds(10));
        base.Dispose();
    }
}
