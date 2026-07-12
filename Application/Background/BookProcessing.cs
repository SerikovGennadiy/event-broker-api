using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Background;

public class BookingHandler : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BookingHandler> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    private const int ITERATION_DELAY = 3;
    private const int EXTERNAL_API_CALL_IMITATION_TIME = 2;
    public BookingHandler(IServiceScopeFactory scopeFactory, ILogger<BookingHandler> logger)
    {
        _serviceScopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cервис обработки бронирования мероприятий запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingBookingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке подтверждений бронирования");

            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(ITERATION_DELAY), stoppingToken);
            }
        }
        _logger.LogInformation("Cервис обработки бронирования мероприятий остановлен");
    }

    private async Task ProcessPendingBookingAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var pendingBookings = await bookingService.GetPendingBookingsAsync();
        if (!pendingBookings.Any())
        {
            _logger.LogInformation("Неподтвержденные брониварония отсутсвуют");
            return;
        }

        _logger.LogInformation("Найдено { Count} ожидающих подтверждения бронирований", pendingBookings.Count());

        var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking.Id, bookingService, eventService, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessBookingAsync(Guid pendingBookingId, IBookingService bookingService, IEventService eventService, CancellationToken stoppingToken)
    {
        var semaphoreAcquired = false;

        try
        {
            // Имитация внешнего вызова
            await Task.Delay(TimeSpan.FromSeconds(EXTERNAL_API_CALL_IMITATION_TIME), stoppingToken);

            await _processingSemaphore.WaitAsync(stoppingToken);
            semaphoreAcquired = true;

            try
            {
                // Проверяем существование бронирования и события
                var booking = await bookingService.GetBookingByIdAsync(pendingBookingId);

                try
                {
                    // Проверяем, что событие существует
                    await eventService.GetEventByIdAsync(booking.EventId);
                }
                catch (Exception)
                {
                    // Событие не найдено - отклоняем бронирование
                    _logger.LogWarning(
                        "Событие {EventId} для бронирования {BookingId} не найдено. Бронирование отклонено",
                        booking.EventId,
                        pendingBookingId);

                    await bookingService.RejectBooingAsync(pendingBookingId);
                    return;
                }

                // Подтверждаем бронирование
                await bookingService.ConfirmBookingAsync(pendingBookingId);
                _logger.LogInformation("Бронирование {BookingId} успешно подтверждено", pendingBookingId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Ошибка при подтверждении бронирования {BookingId}", pendingBookingId);
                await bookingService.RejectBooingAsync(pendingBookingId);
            }
            finally
            {
                if (semaphoreAcquired)
                {
                    _processingSemaphore.Release();
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Обработка бронирования {BookingId} отменена", pendingBookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при обработке бронирования {BookingId}", pendingBookingId);
        }
    }
}
