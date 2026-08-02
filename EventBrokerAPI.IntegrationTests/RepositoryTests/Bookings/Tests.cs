using Application.Common.DTO;
using Application.Common.Extensions;
using Application.Contracts.Persistance;
using Application.Contracts.Services;
using Domain.Exceptions.Booking;
using Domain.Models;
using Infrastructure.Persistence.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace EventBrokerAPI.IntegrationTests.RepositoryTests.Bookings;

public class Tests(Fixture _fixture) : IClassFixture<Fixture>
{
    [Fact]
    public async Task CreateBooking_AddsBookingToDatabase()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var userId = Guid.CreateVersion7();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);
        var @event = Event.Create(title: "Testing", startAt: DateTime.UtcNow, endAt: DateTime.UtcNow.AddDays(1), description: "integration tests", totalSeats: 1);
        repo.Event.CreateEvent(@event);
        await repo.SaveAsync();

        repo.User.CreateUser(User.Restore(userId, "testuser", string.Empty));
        await repo.SaveAsync();

        var booking = new Booking(@event.Id, userId);
        repo.Booking.CreateBooking(booking);
        await repo.SaveAsync();

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var retrievedBooking = await repo.Booking.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(retrievedBooking);
        Assert.Equal(booking.Id, retrievedBooking.Id);
        Assert.Equal(@event.Id, retrievedBooking.EventId);
        Assert.Equal(BookingStatus.Pending, retrievedBooking.Status);
    }

    [Fact]
    public async Task GetBookingById_ReturnsCorrectInformation()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var userId = Guid.CreateVersion7();

        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);
        repo.User.CreateUser(User.Restore(userId, "testuser", string.Empty));
        await repo.SaveAsync();

        var @event = Event.Create(title: "Testing",
                                  startAt: DateTime.UtcNow,
                                  endAt: DateTime.UtcNow.AddDays(1),
                                  description: "integration tests",
                                  totalSeats: 1);
        repo.Event.CreateEvent(@event);
        await repo.SaveAsync();

        Booking booking = new(@event.Id, userId);
        repo.Booking.CreateBooking(booking);
        await repo.SaveAsync();

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var createdBooking = await repo.Booking.GetByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(createdBooking);
        Assert.Equal(createdBooking.Id, booking.Id);
        Assert.Equal(createdBooking.EventId, @event.Id);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task GetAllPendingBookings_ReturnsAllPending()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var userId = Guid.CreateVersion7();
        _fixture.CurrentUser.Setup(x => x.UserId).Returns(userId);
        _fixture.CurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var currentUser = _fixture.CurrentUser.Object;

        using var sp = _fixture.CreateServiceProvider(currentUser);
        using var scope = sp.CreateScope();
       
        var repo = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();
        repo.User.CreateUser(User.Restore(userId, "testuser", string.Empty));
        await repo.SaveAsync();

        Event @event1 = Event.Create("Event 1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), null, 100);
        Event @event2 = Event.Create("Event 2", DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4), null, 100);
        repo.Event.CreateEvent(@event1);
        repo.Event.CreateEvent(@event2);

        var booking1 = new Booking(@event1.Id, userId);
        var booking2 = new Booking(@event1.Id, userId);
        var booking3 = new Booking(@event2.Id, userId);
        var booking4 = new Booking(@event2.Id, userId);

        repo.Booking.CreateBooking(booking1);
        repo.Booking.CreateBooking(booking2);
        repo.Booking.CreateBooking(booking3);
        repo.Booking.CreateBooking(booking4);
        await repo.SaveAsync();

        booking1.Confirm();
        await repo.SaveAsync();

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var result = await repo.Booking.GetAllPendingBookingsAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, b => Assert.Equal(BookingStatus.Pending, b.Status));
    }

    [Fact]
    public async Task GetAllNotExistsPendingBookings__ReturnsEmpty()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        // Act
        var result = await repo.Booking.GetAllPendingBookingsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateBooking_PastEvent_Throws()
    {
        await _fixture.ResetDatabaseAsync();

        var userId = Guid.CreateVersion7();
        _fixture.CurrentUser.Setup(x => x.UserId).Returns(userId);
        _fixture.CurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var currentUser = _fixture.CurrentUser.Object;

        using var sp = _fixture.CreateServiceProvider(currentUser);
        var repo = sp.GetRequiredService<IRepositoryManager>();
        repo.User.CreateUser(User.Restore(userId, "testuser", string.Empty));
        await repo.SaveAsync();

        using var scope = sp.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var pastEvent = new CreateEvent(
            Title: "Past event",
            Description: "already happened",
            StartAt: DateTime.UtcNow.AddDays(-2),
            EndAt: DateTime.UtcNow.AddDays(-1),
            TotalSeats: 10
        );

        var created = await eventService.CreateEventAsync(pastEvent);

        await Assert.ThrowsAsync<BookingPastEventException>(() => bookingService.CreateBookingAsync(created.Id));
    }

    [Fact]
    public async Task CreateBooking_BeyondLimit_Throws()
    {
        await _fixture.ResetDatabaseAsync();
        
        var userId = Guid.CreateVersion7();
        _fixture.CurrentUser.Setup(x => x.UserId).Returns(userId);
        _fixture.CurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var currentUser = _fixture.CurrentUser.Object;

        using var sp = _fixture.CreateServiceProvider(currentUser);
        var repo = sp.GetRequiredService<IRepositoryManager>();
        repo.User.CreateUser(User.Restore(userId, "testuser", string.Empty));
        await repo.SaveAsync();

        using var scope = sp.CreateScope();

        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        // пользователь может иметь максимум 20 активных ожиданий (как в сервисе)
        var createdEvent = await eventService.CreateEventAsync(new CreateEvent(
            Title: "B Event",
            Description: "test",
            StartAt: DateTime.UtcNow.AddDays(1),
            EndAt: DateTime.UtcNow.AddDays(2),
            TotalSeats: 100
        ));


        // Создаём 10 броней согласно лимиту MAX_ACTIVE_BOOKINGS_PER_USER
        var exception = await Record.ExceptionAsync(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                await bookingService.CreateBookingAsync(createdEvent.Id);
            }
        });

        Assert.IsNotType<BookingLimitExceededException>(exception);

        // Попытка создать 11 бронь вызовет исключение
        await Assert.ThrowsAsync<BookingLimitExceededException>(() => bookingService.CreateBookingAsync(createdEvent.Id));
    }

    [Fact]
    public async Task CreateBooking_LimitsPerUser_AreCommonAndIndependentOfEvent()
    {
        await _fixture.ResetDatabaseAsync();

        var userA = Guid.CreateVersion7();
        _fixture.CurrentUser.Setup(x => x.UserId).Returns(userA);
        _fixture.CurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        var currentUser = _fixture.CurrentUser.Object; 
        
        // Пользователь A: создаёт 5 броней
        using var spA = _fixture.CreateServiceProvider(currentUser);
        using var scopeA = spA.CreateScope();
        var repo = scopeA.ServiceProvider.GetRequiredService<IRepositoryManager>();
        repo.User.CreateUser(User.Restore(userA, "testuserA", string.Empty));
        await repo.SaveAsync();

        var eventServiceA = scopeA.ServiceProvider.GetRequiredService<IEventService>();
        var bookingServiceA = scopeA.ServiceProvider.GetRequiredService<IBookingService>();

        // Создаём 10 событий и 10 броней для userA (10 лимит пользователя - антибарыга)
        var events = new List<EventInfo>();
        for (int i = 0; i < 10; i++)
        {
            var ev = new CreateEvent(
                Title: $"A Event #{i}",
                Description: "test",
                StartAt: DateTime.UtcNow.AddDays(1),
                EndAt: DateTime.UtcNow.AddDays(2),
                TotalSeats: 10
            );
            events.Add(await eventServiceA.CreateEventAsync(ev));
            await bookingServiceA.CreateBookingAsync(events[i].Id);
        }

        // Пользователь B: использует тот же DB, но другой userId — должен иметь возможность создать бронь
        var userB = Guid.CreateVersion7();
        _fixture.CurrentUser.Setup(x => x.UserId).Returns(userB);
        _fixture.CurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        currentUser = _fixture.CurrentUser.Object;

        using var spB = _fixture.CreateServiceProvider(currentUser);
        using var scopeB = spB.CreateScope();
        var bookingServiceB = scopeB.ServiceProvider.GetRequiredService<IBookingService>();
        
        repo = scopeB.ServiceProvider.GetRequiredService<IRepositoryManager>();
        repo.User.CreateUser(User.Restore(userB, "testuserB", string.Empty));
        await repo.SaveAsync();

        // Попытка создать бронь на новое событие (или даже на один из существующих) должна пройти для userB
        // Создадим отдельное событие
        var evB = new CreateEvent(
            Title: "B Event",
            Description: "test",
            StartAt: DateTime.UtcNow.AddDays(1),
            EndAt: DateTime.UtcNow.AddDays(2),
            TotalSeats: 10
        );
        var createdForB = await scopeB.ServiceProvider.GetRequiredService<IEventService>().CreateEventAsync(evB);

        var ex = await Record.ExceptionAsync(() => bookingServiceB.CreateBookingAsync(createdForB.Id));
        Assert.Null(ex);
    }
}
