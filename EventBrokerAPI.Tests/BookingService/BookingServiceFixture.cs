using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.ErrorHandling.Exceptions.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repository;
using System.Collections.Concurrent;

namespace EventBrokerAPI.Tests.BookingService;
public class BookingServiceFixture : IAsyncLifetime
{
    public required ServiceProvider serviceProvider;
    public required ConcurrentDictionary<Guid, Event> TestEvents { get; set; } = new();

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        // Регистрируем реальный сервис
        services.AddDbContext<AppDbContext>(options => 
            options.UseInMemoryDatabase($"BookDB_{Guid.CreateVersion7()}"));

        services.AddScoped<IEventService, Service.EventService>();
        services.AddScoped<IBookingService, Service.BookingService>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddLogging(builder =>
        {
            builder.AddConsole();           // Для вывода в консоль
            builder.AddDebug();             // Для вывода в Debug
            builder.AddFilter("Microsoft", LogLevel.Warning); // Фильтры
            builder.AddFilter("System", LogLevel.Warning);
        });

        serviceProvider = services.BuildServiceProvider();

        services.AddLogging(builder =>
        {
            builder.AddConsole();           // Для вывода в консоль
            builder.AddDebug();             // Для вывода в Debug
            builder.AddFilter("Microsoft", LogLevel.Warning); // Фильтры
            builder.AddFilter("System", LogLevel.Warning);
        });
        // Настраиваем делегаты для работы с тестовыми событиями
        Service.BookingService.OnBooked(async data =>
        {
            if (TestEvents.TryGetValue(data.eventId, out var @event))
            {
                if (!@event.TryReserveSeats(data.seats))
                    throw new NoAvailableSeatsException(data.eventId);
            }
            else
            {
                throw new EventNotFoundException(data.eventId);
            }
            await Task.CompletedTask;
        });

        Service.BookingService.OnRejected(async data =>
        {
            if (TestEvents.TryGetValue(data.eventId, out var @event))
            {
                @event.ReleaseSeats(data.seats);
            }
            await Task.CompletedTask;
        });

        await Task.CompletedTask;
    }
    public async Task DisposeAsync()
    {
        #region !
        // обработчики в BookingService регистрируются только один раз из‑за ??= и потом не снимаются,
        // поэтому при прогоне всего набора тестов разные экземпляры фикстуры могут работать с чужими
        // / устаревшими обработчиками(или без них).Исправления — всегда пересоздавать обработчик при
        // регистрации и сбрасывать их при завершении фикстуры.
        // Снимаем статические обработчики, чтобы не ссылаться на объекты фикстуры после её завершения
        // Будут проблемы при запуске нескольких комплектов
        #endregion
        await serviceProvider.DisposeAsync();
        Service.BookingService.ClearHandlers();
    }
}
