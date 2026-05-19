using Entities.Domain.Models;
using Moq;
using Shared.DTO;

namespace EventBrokerAPI.Tests.BookingService.Commands;
public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_ForExistingEvent_ReturnsPendingBooking()
    {
        _fixture.BookingRepositoryMock.Invocations.Clear();
        // Arrange
        var eventId = Guid.NewGuid();

        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventId)).Returns(new Event
        {
            Id = eventId,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        });

        _fixture.MapperMock
            .Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>()))
            .Returns((Booking b) => new BookingDTO(b.Id, b.EventId, b.Status, b.CreatedAt, b.ProcessedAt));

        // Act
        var bookingDto = await _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotNull(bookingDto);
        Assert.Equal(eventId, bookingDto.EventId);
        Assert.Equal(BookingStatus.Pending, bookingDto.Status);

        _fixture.BookingRepositoryMock.Verify(rm => rm.CreateBooking(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBookings_UniqueIds_AllCreated()
    {
        _fixture.RepositoryManagerMock.Invocations.Clear(); 
        // Arrange
        var eventId = Guid.NewGuid();
        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventId)).Returns(new Event
        {
            Id = eventId,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        });

        _fixture.MapperMock
            .Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>()))
            .Returns((Booking b) => new BookingDTO(b.Id, b.EventId, b.Status, b.CreatedAt, b.ProcessedAt));

        // Act
        var first = await _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var second = await _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(BookingStatus.Pending, first.Status);
        Assert.Equal(BookingStatus.Pending, second.Status);

        _fixture.RepositoryManagerMock.Verify(rm => rm.Booking.CreateBooking(It.IsAny<Booking>()), Times.Exactly(2));
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task GetBooking_ChangeStatus_ReturnCorrectStatus()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var booking = new Booking(bookingId, Guid.NewGuid());

        _fixture.BookingRepositoryMock.Setup(r => r.GetById(bookingId)).Returns(booking);

        _fixture.MapperMock
            .Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>()))
            .Returns((Booking b) => new BookingDTO(b.Id, b.EventId, b.Status, b.CreatedAt, b.ProcessedAt));

        // Act - Confirm
        _fixture.BookingService.ConfirmBooking(bookingId);
        var confirmed = await _fixture.BookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert Confirmed
        Assert.Equal(BookingStatus.Confirmed, confirmed.Status);
        Assert.NotNull(confirmed.ProcessedAt);

        // Act - Reset status to Pending then Reject
        booking.OnPending();

        _fixture.BookingService.RejectBooking(bookingId);
        var rejected = await _fixture.BookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);

        // Assert Rejected
        Assert.Equal(BookingStatus.Rejected, rejected.Status);
        Assert.NotNull(rejected.ProcessedAt);
    }
}