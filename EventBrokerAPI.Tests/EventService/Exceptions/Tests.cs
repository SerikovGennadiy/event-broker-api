using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using EventBrokerAPI.Tests.Fixture.EventService;
using Microsoft.Extensions.DependencyInjection;
using Shared.DTO;
using Shared.ModelExtensions;

namespace EventBrokerAPI.Tests.EventService.Exceptions;

public class Tests(EventServiceFixture _fixture) : IClassFixture<EventServiceFixture>
{
    [Fact]
    [Trait("Event", "Exceptions")]
    public async Task GetEventById_NotExistId_ThrowsEventNotFoundException()
    {
        // Arrange
        var unexistingGuid = Guid.NewGuid();

        // Act
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var exception = await Record.ExceptionAsync(() => eventService.GetEventByIdAsync(unexistingGuid));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<EventNotFoundException>(exception);
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public async Task UpdateEvent_NotExistId_ThrowsEventNotFoundException()
    {
        // Arrange
        var unexistingGuid = Guid.NewGuid();
        var dto = new EventDTO("Title", "Description", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), TotalSeats: 10);

        // Act & Assert
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        await Assert.ThrowsAsync<EventNotFoundException>(() => eventService.UpdateEventAsync(unexistingGuid, dto));
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public async Task CreateEvent_IncorrectTitle_ThrowsEventNoTitleException()
    {
        // Arrange
        var eventDTO = new CreateEvent(Title: string.Empty, // некорректный заголовок
                                       Description: "Info about event",
                                       StartAt: DateTime.UtcNow,
                                       EndAt: DateTime.UtcNow.AddDays(1),
                                       TotalSeats: 10);
        // Act & Assert
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var expeption = await Assert.ThrowsAsync<EventNoTitleException>(() => eventService.CreateEventAsync(eventDTO));
        Assert.Equal("Отсуствует наименование события", expeption.Message);
    }

    [Fact]
    [Trait("Event", "Exceptions")]
    public async Task CreateEvent_IncorrectTotalSeats_ThrowsEventBadTotalSeatsQuantity()
    {
        // Arrange
        var eventDTO = new CreateEvent(Title: "Event without seats", // некорректный заголовок
                                       Description: "Info about event",
                                       StartAt: DateTime.UtcNow,
                                       EndAt: DateTime.UtcNow.AddDays(1),
                                       TotalSeats: 0);
        // Act & Assert
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var expeption = await Assert.ThrowsAsync<EventBadTotalSeatsQuantity>(() => eventService.CreateEventAsync(eventDTO));
        Assert.Equal("Общее количество мест на мероприятии должно быть больше 0", expeption.Message);
    }


    [Fact]
    [Trait("Event", "Exceptions")]
    public async Task UpdateEvent_IncorrectDateRange_ThrowsEventBadDateRangeException()
    {
        Guid eventGuid = Guid.NewGuid();

        var original = Event.Create(title: "Existing Event",
                                    startAt: DateTime.UtcNow,
                                    endAt: DateTime.UtcNow.AddDays(1),
                                    default,
                                    totalSeats: 10);
        Guid eventId = original.Id;

        Event updated = Event.Create(title: "Another one super event",
                                     startAt: DateTime.UtcNow,
                                     endAt: DateTime.UtcNow.AddDays(-2),
                                     description: "Info about event",
                                     totalSeats: 10);

        EventDTO updatedEventDTO = updated.toDTO();

        // Act & Assert
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var exception = await  Assert.ThrowsAsync<EventBadDateRangeException>(() => eventService.UpdateEventAsync(eventId, updatedEventDTO));
        Assert.Equal("Некорректные даты начала и завершения мероприятия", exception.Message);
    }
}
