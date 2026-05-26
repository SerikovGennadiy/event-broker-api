using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Moq;
using Shared.DTO;
using System.Collections.Concurrent;

namespace EventBrokerAPI.Tests.BookingService.Concurrency;

[Collection("BookingServiceTests")]
public class Tests(BookingServiceFixture fixture) : IClassFixture<BookingServiceFixture>
{
    private readonly BookingServiceFixture _fixture = fixture;

    [Fact]
    [Trait("Booking", "Concurrency")]
    public async Task ConcurrentBookings_WithOverbooking_OnlyAllowsUpToCapacity()
    {
        // Arrange
        _fixture.ResetAllMocks();

        var concurrentRequests = 20;
        var eventId = Guid.NewGuid();
        var totalSeats = 5;
        Event testEvent = CreateTestEvent(eventId, totalSeats);

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


        var successCount = 0;
        var failureCount = 0;
        var lockObj = new object();

        var capturedBookings = new ConcurrentBag<Booking>();
        _fixture.BookingRepositoryMock
            .Setup(r => r.CreateBooking(It.IsAny<Booking>()))
            .Callback<Booking>(b => capturedBookings.Add(b));

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests).Select(async _ =>
        {
            try
            {
                await _fixture.BookingService.CreateBookingAsync(
                    eventId,
                    CancellationToken.None
                );
                Interlocked.Increment(ref successCount);
            }
            catch (NoAvailableSeatsException)
            {
                Interlocked.Increment(ref failureCount);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalSeats, successCount);
        Assert.Equal(concurrentRequests - totalSeats, failureCount);
        Assert.Equal(0, testEvent.AvailableSeats);
        Assert.Equal(totalSeats, capturedBookings.Count);
    }

    [Fact]
    [Trait("Booking", "Concurrency")]
    public async Task ConcurrentBookings_AllHaveUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var totalSeats = 10;
        var testEvent = CreateTestEvent(totalSeats: totalSeats, eventId: eventId);

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
        _fixture.BookingRepositoryMock
            .Setup(r => r.CreateBooking(It.IsAny<Booking>()))
            .Callback<Booking>(b => bookingIds.Add(b.Id));

        // Act
        var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
        {
            await _fixture.BookingService.CreateBookingAsync(
                eventId,
                CancellationToken.None
            );
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalSeats, bookingIds.Count);
        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
        Assert.All(bookingIds, id => Assert.NotEqual(Guid.Empty, id));
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