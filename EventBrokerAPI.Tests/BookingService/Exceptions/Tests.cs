using Microsoft.Extensions.DependencyInjection;

namespace EventBrokerAPI.Tests.BookingService.Exceptions;

[Collection("BookingServiceTests")]
public class Tests(Fixture fixture) : IClassFixture<Fixture>
{
    private readonly Fixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task CreateBooking_ForNonExistingOrDeletedEvent_ThrowsEventNotFoundException()
    {
        // Arrange
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var notExistEventGuid = Guid.NewGuid();

        // Act
        var ex = await Record.ExceptionAsync(() => bookingService.CreateBookingAsync(notExistEventGuid));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<EventNotFoundException>(ex);
    }

    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task GetBookingById_NonExistingId_ThrowsBookingNotFoundException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act & Assert
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(bookingId));
    }

    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task CreateBooking_WhenNoSeatsAvailable_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var onlyOneSeat = 1;

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(onlyOneSeat);
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Занимаем единственное место
        await bookingService.CreateBookingAsync(@event.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => bookingService.CreateBookingAsync(@event.Id)
        );
    }

    private static CreateEvent CreateTestEvent(int totalSeats = 10)
    {
        return new CreateEvent(
            Title: "Test event",
            Description: "Initial description",
            StartAt: DateTime.UtcNow,
            EndAt: DateTime.UtcNow.AddDays(1),
            TotalSeats: totalSeats
        );
    }
}
