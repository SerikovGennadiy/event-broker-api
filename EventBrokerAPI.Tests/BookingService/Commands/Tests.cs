using Entities.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTO;
using System.Collections.Concurrent;

namespace EventBrokerAPI.Tests.BookingService.Commands;

[Collection("BookingServiceTests")]
public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_ForExistingEvent_ReturnsPendingBooking()
    {
        _fixture.ResetAllMocks();
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event
        {
            Id = eventId,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            // при default (0) тест падает (не хватает мест)
            TotalSeats = 10,
            AvailableSeats = 10
        };

        _fixture.TestEvents[eventId] = @event;

        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventId)).Returns(@event);

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
        _fixture.ResetAllMocks();
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event
        {
            Id = eventId,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            // при default (0) тест падает (не хватает мест)
            TotalSeats = 10,
            AvailableSeats = 10
        };

        _fixture.TestEvents[eventId] = @event;

        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventId)).Returns(@event);

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

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task RejectBooking_AllowsNewBooking_OnSameSeat()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var testEvent = CreateTestEvent(eventId, totalSeats: 1);

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

        var capturedBookings = new List<Booking>();
        _fixture.BookingRepositoryMock
            .Setup(r => r.CreateBooking(It.IsAny<Booking>()))
            .Callback<Booking>(b =>
            {
                capturedBookings.Add(b);
                _fixture.BookingRepositoryMock
                    .Setup(r => r.GetById(b.Id))
                    .Returns(b);
            });

        // Act - создаем бронь и отменяем её
        var firstBooking = await _fixture.BookingService.CreateBookingAsync(
            eventId,
            CancellationToken.None
        );

        _fixture.BookingService.RejectBooking(firstBooking.Id);

        // Создаем новую бронь после отмены
        var secondBooking = await _fixture.BookingService.CreateBookingAsync(
            eventId,
            CancellationToken.None
        );

        // Assert
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        Assert.Equal(0, testEvent.AvailableSeats);
        Assert.Equal(BookingStatus.Rejected, capturedBookings.First().Status);
        Assert.Equal(BookingStatus.Pending, secondBooking.Status);
    }


    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_DecreasesAvailableSeats_ByOne()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var testEvent = CreateTestEvent(eventId, totalSeats: 10);

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
       
        // Act
        var booking = await _fixture.BookingService.CreateBookingAsync(
            eventId,
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(9, testEvent.AvailableSeats);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(eventId, booking.EventId);

        _fixture.BookingRepositoryMock.Verify(
            r => r.CreateBooking(It.IsAny<Booking>()),
            Times.Once
        );
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateMultipleBookings_UpToLimit_AllSuccessfulWithUniqueIds()
    {
        _fixture.ResetAllMocks();
        // Arrange
        // Arrange
        var eventId = Guid.NewGuid();
        var totalSeats = 5;
        var testEvent = CreateTestEvent(eventId, totalSeats);

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

        var bookingIds = new ConcurrentBag<Guid>();

        // Act
        var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
        {
            var booking = await _fixture.BookingService.CreateBookingAsync(
                eventId,
                CancellationToken.None
            );
            bookingIds.Add(booking.Id);
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalSeats, bookingIds.Count);
        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
        Assert.Equal(0, testEvent.AvailableSeats);
        _fixture.BookingRepositoryMock.Verify(
           r => r.CreateBooking(It.IsAny<Booking>()),
          Times.Exactly(totalSeats)
       );
    }
    private static Event CreateTestEvent(Guid eventId, int totalSeats)
    {
        return new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };
    }
}