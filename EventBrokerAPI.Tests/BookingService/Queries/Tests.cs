using Moq;
using Shared.DTO;
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
        var bookingId = Guid.NewGuid();
        var booking = new Booking(bookingId, Guid.NewGuid());

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var result = await bookingService.GetBookingByIdAsync(bookingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookingId, result.Id);
        Assert.Equal(booking.EventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }
}