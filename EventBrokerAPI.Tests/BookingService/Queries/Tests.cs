using Contracts.Service;
using Entities.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EventBrokerAPI.Tests.BookingService.Queries;

[Collection("BookingServiceTests")]
public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Queries")]
    public async Task GetBookingById_ReturnsCorrectInformation()
    {
        // Arrange
        var tempEvent = CreateTestEvent(totalSeats: 5);

        _fixture.TestEvents[tempEvent.Id] = tempEvent;

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var booking = await bookingService.CreateBookingAsync(tempEvent.Id);
        var result = await bookingService.GetBookingByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.EventId, tempEvent.Id);
        Assert.Equal(BookingStatus.Pending, booking.Status);
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