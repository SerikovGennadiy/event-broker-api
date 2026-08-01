using Application.Common.DTO;
using Application.Contracts.Services;
using Application.Contracts.Services.Auth;
using Domain.Exceptions.Booking;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Concurrent;

namespace EventBrokerAPI.Tests.BookingService.Commands;

[Collection("BookingServiceTests")]
public class Tests : IClassFixture<Fixture>
{
    private readonly Fixture _fixture; 
    public Tests(Fixture fixture)
    {
        _fixture = fixture;
        _fixture.RecreateDatabase();
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_ForExistingEvent_ReturnsPendingBooking()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent();
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act
        var bookingDto = await bookingService.CreateBookingAsync(@event.Id);

        // Assert
        Assert.NotNull(bookingDto);
        Assert.Equal(@event.Id, bookingDto.EventId);
        Assert.Equal(BookingStatus.Pending, bookingDto.Status);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBookings_UniqueIds_AllCreated()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent();
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act
        var first = await bookingService.CreateBookingAsync(@event.Id);
        var second = await bookingService.CreateBookingAsync(@event.Id);

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
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats: 5);
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act
        var first = await bookingService.CreateBookingAsync(@event.Id);
        var second = await bookingService.CreateBookingAsync(@event.Id);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(BookingStatus.Pending, first.Status);
        Assert.Equal(BookingStatus.Pending, second.Status);

        // Act 
        var firstBooking = await bookingService.CreateBookingAsync(@event.Id);
        await bookingService.ConfirmBookingAsync(firstBooking.Id);
        var confiredDTO = await bookingService.GetBookingByIdAsync(firstBooking.Id);

        var secondBooking = await bookingService.CreateBookingAsync(@event.Id);
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
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats: 1);
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act - создаем бронь и отменяем её
        var firstBooking = await bookingService.CreateBookingAsync(@event.Id);

        await bookingService.RejectBooingAsync(firstBooking.Id);
        var rejectedDTO = await bookingService.GetBookingByIdAsync(firstBooking.Id);

        var secondBooking = await bookingService.CreateBookingAsync(@event.Id);
        var pendingDTO = await bookingService.GetBookingByIdAsync(secondBooking.Id);

        var offEvent = await eventService.GetEventByIdAsync(@event.Id);
        // Assert
        Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        Assert.Equal(0, offEvent.AvailableSeats);
        Assert.Equal(BookingStatus.Rejected, rejectedDTO.Status);
        Assert.Equal(BookingStatus.Pending, pendingDTO.Status);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateBooking_DecreasesAvailableSeats_ByOne()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent();
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act
        var booking = await bookingService.CreateBookingAsync(@event.Id);
        var updatedEvent = await eventService.GetEventByIdAsync(@event.Id);
        // Assert
        Assert.NotNull(booking);
        Assert.Equal(9, updatedEvent.AvailableSeats);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Equal(@event.Id, booking.EventId);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CreateMultipleBookings_UpToLimit_AllSuccessfulWithUniqueIds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var totalSeats = 5;
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats);
        var @event = await eventService.CreateEventAsync(eventDTO);

        var bookingIds = new ConcurrentBag<Guid>();

        // Act
        var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
        {
            var booking = await bookingService.CreateBookingAsync(@event.Id);
            bookingIds.Add(booking.Id);
        });

        await Task.WhenAll(tasks);

        var updatedEvent = await eventService.GetEventByIdAsync(@event.Id);
        // Assert
        Assert.Equal(totalSeats, bookingIds.Count);
        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
        Assert.Equal(0, updatedEvent.AvailableSeats);
    }

    [Fact]
    [Trait("Booking", "Commands")]
    public async Task CancelBooking_WhenCalledByOwner_ChangesStatusToCancelled_AndReleasesSeat()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        await _fixture.InitProviderWithUserContext(userId);

        var totalSeats = 1;
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var bookingService = _fixture.serviceProvider.GetRequiredService<IBookingService>();
        var eventDTO = CreateTestEvent(totalSeats);
        var @event = await eventService.CreateEventAsync(eventDTO);

        // Act - создаем бронь
        var booking = await bookingService.CreateBookingAsync(@event.Id);

        // Убедимся, что место занято
        var afterCreateEvent = await eventService.GetEventByIdAsync(@event.Id);
        Assert.Equal(0, afterCreateEvent.AvailableSeats);

        // Act - отменяем бронь владельцем
        var result = await bookingService.CancelBookingAsync(booking.Id);

        // Assert
        Assert.True(result);
        var cancelledDTO = await bookingService.GetBookingByIdAsync(booking.Id);
        var updatedEvent = await eventService.GetEventByIdAsync(@event.Id);

        Assert.Equal(BookingStatus.Cancelled, cancelledDTO.Status);
        Assert.NotNull(cancelledDTO.ProcessedAt);
        Assert.Equal(totalSeats, updatedEvent.AvailableSeats);
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