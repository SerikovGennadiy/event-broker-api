using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.ErrorHandling.Exceptions.Event;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.DTO;

namespace EventBrokerAPI.Tests.BookingService.Exceptions;

[Collection("BookingServiceTests")]
public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;
    
    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task CreateBooking_ForNonExistingOrDeletedEvent_ThrowsEventNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var ex = await Record.ExceptionAsync(() => bookingService.CreateBookingAsync(eventId));

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
        var testEvent = CreateTestEvent(totalSeats: onlyOneSeat);
        var eventId = testEvent.Id;

        _fixture.TestEvents[eventId] = testEvent;

        // Занимаем единственное место
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        await bookingService.CreateBookingAsync(eventId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => bookingService.CreateBookingAsync(eventId)
        );
    }

    private static Event CreateTestEvent(int totalSeats)
    {
        return Event.Create(title: "Test Event",
                            startAt: DateTime.UtcNow,
                            endAt: DateTime.UtcNow.AddDays(1),
                            description: "Test Description",
                            totalSeats: totalSeats);
    }
}
