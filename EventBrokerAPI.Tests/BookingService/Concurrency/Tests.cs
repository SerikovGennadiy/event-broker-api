using Moq;
using Shared.DTO;
using System.Collections.Concurrent;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Microsoft.Extensions.DependencyInjection;

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
        var concurrentRequests = 20;
        var totalSeats = 5;

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats);
        var @event = await eventService.CreateEventAsync(eventDTO);

        var successCount = 0;
        var failureCount = 0;

        var capturedBookings = new ConcurrentBag<BookingDTO>();

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests).Select(_ => Task.Run(async () =>
        {
            try
            {
                var createdBooking = await bookingService.CreateBookingAsync(@event.Id);
                capturedBookings.Add(createdBooking);

                Interlocked.Increment(ref successCount);
            }
            catch (NoAvailableSeatsException)
            {
                Interlocked.Increment(ref failureCount);
            }
        }));

        await Task.WhenAll(tasks);

        var updatedEvent = await eventService.GetEventByIdAsync(@event.Id);
        // Assert
        Assert.Equal(totalSeats, successCount);
        Assert.Equal(concurrentRequests - totalSeats, failureCount);
        Assert.Equal(0, updatedEvent.AvailableSeats);
        Assert.Equal(totalSeats, capturedBookings.Count);
    }

    [Fact]
    [Trait("Booking", "Concurrency")]
    public async Task ConcurrentBookings_AllHaveUniqueIds()
    {
        // Arrange
        var totalSeats = 10;
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats);
        var @event = await eventService.CreateEventAsync(eventDTO);

        var bookingIds = new ConcurrentBag<Guid>();

        // Act
        var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
        {
            var createdBooking = await bookingService.CreateBookingAsync(@event.Id);
            bookingIds.Add(createdBooking.Id);
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalSeats, bookingIds.Count);
        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
        Assert.All(bookingIds, id => Assert.NotEqual(Guid.Empty, id));
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