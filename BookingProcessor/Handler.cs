using Contracts.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingProcessor;

public class Handler : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<Handler> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    public Handler(IServiceScopeFactory scopeFactory, ILogger<Handler> logger)
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
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        _logger.LogInformation("Cервис обработки бронирования мероприятий остановлен");
    }

    private async Task ProcessPendingBookingAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var pendingBookings = bookingService.GetPendingBookings().ToList();
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
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            await _processingSemaphore.WaitAsync(stoppingToken);
            semaphoreAcquired = true;

            try
            {
                // Проверяем существование бронирования и события
                var booking = await bookingService.GetBookingByIdAsync(pendingBookingId, stoppingToken);

                try
                {
                    // Проверяем, что событие существует
                    eventService.GetEventById(booking.EventId);
                }
                catch (Exception)
                {
                    // Событие не найдено - отклоняем бронирование
                    _logger.LogWarning(
                        "Событие {EventId} для бронирования {BookingId} не найдено. Бронирование отклонено",
                        booking.EventId,
                        pendingBookingId);

                    bookingService.RejectBooking(pendingBookingId);
                    return;
                }

                // Подтверждаем бронирование
                bookingService.ConfirmBooking(pendingBookingId);
                _logger.LogInformation("Бронирование {BookingId} успешно подтверждено", pendingBookingId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Ошибка при подтверждении бронирования {BookingId}", pendingBookingId);
                bookingService.RejectBooking(pendingBookingId);
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


