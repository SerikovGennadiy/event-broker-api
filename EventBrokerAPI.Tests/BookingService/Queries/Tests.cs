using Entities.Domain.Models;
using Moq;
using Shared.DTO;

namespace EventBrokerAPI.Tests.BookingService.Queries;

public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Queries")]
    public async Task GetBookingById_ReturnsCorrectInformation()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            EventId = Guid.NewGuid(),
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _fixture.BookingRepositoryMock.Setup(r => r.GetById(bookingId)).Returns(booking);

        _fixture.MapperMock
            .Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>()))
            .Returns((Booking b) => new BookingDTO(b.Id, b.EventId, b.Status, b.CreatedAt, b.ProcessedAt));

        // Act
        var result = await _fixture.BookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookingId, result.Id);
        Assert.Equal(booking.EventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }
}