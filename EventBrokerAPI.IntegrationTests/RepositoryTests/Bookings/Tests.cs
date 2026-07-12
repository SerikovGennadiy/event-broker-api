using Domain.Models;
using Infrastructure.Persistence.Repository;

namespace EventBrokerAPI.IntegrationTests.RepositoryTests.Bookings;

public class Tests(Fixture _fixture) : IClassFixture<Fixture>
{
    [Fact]
    public async Task CreateBooking_AddsBookingToDatabase()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);
        var @event = Event.Create(title: "Testing", startAt: DateTime.UtcNow, endAt: DateTime.UtcNow.AddDays(1), description: "integration tests", totalSeats: 1);
        repo.Event.CreateEvent(@event);
        await repo.SaveAsync();

        var booking = new Booking(@event.Id);
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

        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);
        var @event = Event.Create(title: "Testing",
                                  startAt: DateTime.UtcNow,
                                  endAt: DateTime.UtcNow.AddDays(1),
                                  description: "integration tests",
                                  totalSeats: 1);
        repo.Event.CreateEvent(@event);
        await repo.SaveAsync();

        Booking booking = new(@event.Id);
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

        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        var @event1 = Event.Create("Event 1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), null, 100);
        var @event2 = Event.Create("Event 2", DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4), null, 100);
        repo.Event.CreateEvent(@event1);
        repo.Event.CreateEvent(@event2);

        await repo.SaveAsync();

        var booking1 = new Booking(@event1.Id);
        var booking2 = new Booking(@event1.Id);
        var booking3 = new Booking(@event2.Id);
        var booking4 = new Booking(@event2.Id);

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
}
