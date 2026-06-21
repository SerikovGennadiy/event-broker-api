using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Service;
using Shared.DTO;
using System.Collections.Concurrent;

namespace EventBrokerAPI.Tests.BookingService.Commands;

[Collection("BookingServiceTests")]
public class Tests(BookingServiceFixture _fixture) : IClassFixture<BookingServiceFixture>
{
    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_ForExistingEvent_ReturnsPendingBooking()
    {
        // Arrange
        var @event = Event.Create(title: "Test",
                                  startAt: DateTime.UtcNow,
                                  endAt: DateTime.UtcNow.AddDays(1),
                                  description: default,
                                  totalSeats: 10);

        var eventId = @event.Id;

        _fixture.TestEvents[eventId] = @event;

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var bookingDto = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotNull(bookingDto);
        Assert.Equal(eventId, bookingDto.EventId);
        Assert.Equal(BookingStatus.Pending, bookingDto.Status);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBookings_UniqueIds_AllCreated()
    {
        // Arrange
        var @event = Event.Create(title: "Test",
                                startAt: DateTime.UtcNow,
                                endAt: DateTime.UtcNow.AddDays(1),
                                description: default,
                                totalSeats: 10);

        var eventId = @event.Id;

        _fixture.TestEvents[eventId] = @event;

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var first = await bookingService.CreateBookingAsync(eventId);
        var second = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(BookingStatus.Pending, first.Status);
        Assert.Equal(BookingStatus.Pending, second.Status);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task GetBooking_ChangeStatus_ReturnCorrectStatus()
    {
        // Arrange
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var tempEvent = CreateTestEvent(totalSeats: 5);
        var eventId = tempEvent.Id;

        // необязательно
        _fixture.TestEvents[eventId] = tempEvent;

        // Act 
        var firstBooking = await bookingService.CreateBookingAsync(eventId);
        await bookingService.ConfirmBookingAsync(firstBooking.Id);
        var confiredDTO = await bookingService.GetBookingByIdAsync(firstBooking.Id);

        var secondBooking = await bookingService.CreateBookingAsync(eventId);
        await bookingService.RejectBooingAsync(secondBooking.Id);
        var rejectedDTO = await bookingService.GetBookingByIdAsync(secondBooking.Id);

        // Assert 
        Assert.Equal(BookingStatus.Confirmed, confiredDTO.Status);
        Assert.NotNull(confiredDTO.ProcessedAt);
        await Assert.ThrowsAsync<BookingNoReverseStatus>(() => bookingService.RejectBooingAsync(confiredDTO.Id));

        Assert.Equal(BookingStatus.Rejected, rejectedDTO.Status);
        Assert.NotNull(rejectedDTO.ProcessedAt);
        await Assert.ThrowsAsync<BookingNoReverseStatus>(() => bookingService.ConfirmBookingAsync(rejectedDTO.Id));
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task RejectBooking_AllowsNewBooking_OnSameSeat()
    {
        // Arrange
        var bookingSerivce = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var testEvent = CreateTestEvent(totalSeats: 1);
        var eventId = testEvent.Id;

        _fixture.TestEvents[eventId] = testEvent;

        // Act - создаем бронь и отменяем её
        var firstBooking = await bookingSerivce.CreateBookingAsync(eventId);

        await bookingSerivce.RejectBooingAsync(firstBooking.Id);
        var rejectedDTO = await bookingSerivce.GetBookingByIdAsync(firstBooking.Id);

        var secondBooking = await bookingSerivce.CreateBookingAsync(eventId);
        var pendingDTO = await bookingSerivce.GetBookingByIdAsync(secondBooking.Id);

        // Assert
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        Assert.Equal(0, testEvent.AvailableSeats);
        Assert.Equal(BookingStatus.Rejected, rejectedDTO.Status);
        Assert.Equal(BookingStatus.Pending, pendingDTO.Status);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_DecreasesAvailableSeats_ByOne()
    {
        // Arrange
        var testEvent = CreateTestEvent(totalSeats: 10);
        var eventId = testEvent.Id;

        _fixture.TestEvents[eventId] = testEvent;

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var booking = await bookingService.CreateBookingAsync(eventId);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(9, testEvent.AvailableSeats);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(eventId, booking.EventId);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateMultipleBookings_UpToLimit_AllSuccessfulWithUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var totalSeats = 5;
        var testEvent = CreateTestEvent(totalSeats);

        _fixture.TestEvents[eventId] = testEvent;

        var bookingIds = new ConcurrentBag<Guid>();

        // Act
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
        {
            var booking = await bookingService.CreateBookingAsync(eventId);
            bookingIds.Add(booking.Id);
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(totalSeats, bookingIds.Count);
        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
        Assert.Equal(0, testEvent.AvailableSeats);
    }

    private static Event CreateTestEvent(int totalSeats)
    {
        return Event.Create(title: "Test",
                            startAt: DateTime.UtcNow,
                            endAt: DateTime.UtcNow.AddDays(1),
                            description: default,
                            totalSeats: totalSeats);
    }
}