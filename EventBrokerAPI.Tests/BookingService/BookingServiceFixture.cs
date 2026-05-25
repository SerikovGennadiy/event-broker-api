using AutoMapper;
using Contracts.Repository;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.ErrorHandling.Exceptions.Event;
using Moq;
using System.Collections.Concurrent;
using Xunit;
using BookingServiceType = Service.BookingService;

namespace EventBrokerAPI.Tests.BookingService;
public class BookingServiceFixture : IAsyncLifetime
{
    public required BookingServiceType BookingService { get; set; }
    public required Mock<IRepositoryManager> RepositoryManagerMock { get; set; } 
    public required Mock<IEventRepository> EventRepositoryMock { get; set;} 
    public required Mock<IBookingRepository> BookingRepositoryMock { get; set;} 
    public required Mock<IMapper> MapperMock { get; set;} 
    public required ConcurrentDictionary<Guid, Event> TestEvents { get; set; }

    public async Task InitializeAsync()
    {
        // Создаем новые инстансы для каждого запуска

        RepositoryManagerMock = new Mock<IRepositoryManager>();
        EventRepositoryMock = new Mock<IEventRepository>();
        BookingRepositoryMock = new Mock<IBookingRepository>();
        MapperMock = new Mock<IMapper>();
        TestEvents = new ConcurrentDictionary<Guid, Event>();

        RepositoryManagerMock
            .Setup(x => x.Event)
            .Returns(EventRepositoryMock.Object);

        RepositoryManagerMock
            .Setup(x => x.Booking)
            .Returns(BookingRepositoryMock.Object);

        BookingService = new BookingServiceType(
            RepositoryManagerMock.Object,
            MapperMock.Object
        );

        // Важно: очищаем делегаты, связывающиеся через DI!!
        BookingServiceType.ClearHandlers();
        
        // Настраиваем делегаты для работы с тестовыми событиями
        BookingServiceType.OnBooked(data =>
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
        });

        BookingServiceType.OnRejected(data =>
        {
            if (TestEvents.TryGetValue(data.eventId, out var @event))
            {
                @event.ReleaseSeats(data.seats);
            }
        });

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Очищаем статические делегаты
        BookingServiceType.ClearHandlers();

        // Очищаем моки
        RepositoryManagerMock?.Reset();
        EventRepositoryMock?.Reset();
        BookingRepositoryMock?.Reset();
        MapperMock?.Reset();

        // Очищаем тестовые данные
        TestEvents?.Clear();

        await Task.CompletedTask;
    }

    public void ResetAllMocks()
    {
        RepositoryManagerMock.Reset();
        EventRepositoryMock.Reset();
        BookingRepositoryMock.Reset();
        MapperMock.Reset();

        // Перенастраиваем базовые связи
        RepositoryManagerMock
            .Setup(x => x.Event)
            .Returns(EventRepositoryMock.Object);
        RepositoryManagerMock
            .Setup(x => x.Booking)
            .Returns(BookingRepositoryMock.Object);
    }
}
