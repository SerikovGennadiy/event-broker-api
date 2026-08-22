using Application.Common.DTO;
using Application.Contracts.Services;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EventBrokerAPI.Tests.BookingService.Queries;

[Collection("BookingServiceTests")]
public class Tests(Fixture fixture) : IClassFixture<Fixture>
{
    private readonly Fixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Queries")]
    public async Task GetBookingById_ReturnsCorrectInformation()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats: 5);
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act
        var booking = await bookingService.CreateBookingAsync(@event.Id);
        var result = await bookingService.GetBookingByIdAsync(booking.Id);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.EventId, @event.Id);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    private static CreateEvent CreateTestEvent(int totalSeats = 10)
    {
        // нельзя бронировать событие, которое уже прошло, поэтому устанавливаем дату начала в будущем
        return new CreateEvent(
            Title: "Test event",
            Description: "Initial description",
            StartAt: DateTime.UtcNow.AddDays(10),
            EndAt: DateTime.UtcNow.AddDays(15),
            TotalSeats: totalSeats
        );
    }
}