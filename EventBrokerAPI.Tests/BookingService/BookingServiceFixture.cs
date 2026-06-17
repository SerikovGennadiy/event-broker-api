using Moq;
using AutoMapper;
using Contracts.Service;
using Contracts.Repository;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using Entities.ErrorHandling.Exceptions.Booking;
using Microsoft.EntityFrameworkCore;
using Repository;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace EventBrokerAPI.Tests.BookingService;
public class BookingServiceFixture : IAsyncLifetime
{
    public required ServiceProvider serviceProvider;
    public required ConcurrentDictionary<Guid, Event> TestEvents { get; set; }

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

        serviceProvider = services.BuildServiceProvider();

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
        await serviceProvider.DisposeAsync();
    }
}
