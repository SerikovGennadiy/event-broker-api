using Contracts.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingProcessor;

public class Handler : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<Handler> _logger;
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
                await ConfirmPendingBookingsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch(Exception ex)
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

    private async Task ConfirmPendingBookingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceManager = scope.ServiceProvider.GetRequiredService<IServiceManager>();

        var pendingBookings = serviceManager.BookingService.GetPendingBookings();

        if (!pendingBookings.Any())
        {
            _logger.LogInformation("Неподтвержденные бронирования отсутсвуют");
            return;
        }

        _logger.LogInformation("Найдено {Count} ожидающих подтверждения бронирований", pendingBookings.Count());

        foreach (var pendingBook in pendingBookings)
        {
            try
            {
                serviceManager.BookingService.ConfirmBooking(pendingBook.Id);
                _logger.LogInformation("Бронирование {BookingId} успешно подтверждено", pendingBook.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось подтвердить бронирование {BookingId}", pendingBook.Id);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
