using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.ErrorHandling.Exceptions.Event;
using Moq;
using Shared.DTO;

namespace EventBrokerAPI.Tests.BookingService.Exceptions;

[Collection("BookingServiceTests")]
public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;
    
    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task CreateBooking_ForNonExistingEvent_ThrowsEventNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventId)).Returns((Event?)null);

        // Act
        var ex = await Record.ExceptionAsync(() => _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<EventNotFoundException>(ex);
    }

    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task CreateBooking_ForDeletedEvent_ThrowsEventNotFoundException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        // Симулируем удалённое событие тем же поведением репозитория (null)
        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventId)).Returns((Event?)null);

        // Act
        var ex = await Record.ExceptionAsync(() => _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None));

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
        _fixture.BookingRepositoryMock.Setup(r => r.GetById(bookingId)).Returns((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BookingNotFoundException>(() => _fixture.BookingService.GetBookingByIdAsync(bookingId, CancellationToken.None));
    }

    [Fact]
    [Trait("Booking", "Exceptions")]
    public async Task CreateBooking_WhenNoSeatsAvailable_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var onlyOneSeat = 1;
        var testEvent = new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = onlyOneSeat,
            AvailableSeats = onlyOneSeat
        };

        // Настраиваем маппер
        _fixture.MapperMock
            .Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>()))
            .Returns((Booking b) => new BookingDTO(
                b.Id,
                b.EventId,
                b.Status,
                b.CreatedAt,
                b.ProcessedAt
            ));

        _fixture.TestEvents[eventId] = testEvent;

        // Занимаем единственное место
        await _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => _fixture.BookingService.CreateBookingAsync(eventId, CancellationToken.None)
        );
    }

}
