using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using EventBrokerAPI.Tests.Fixture.EventService;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.DTO;
using Shared.ModelExtensions;

namespace EventBrokerAPI.Tests.EventService.Commands;

public class Tests(EventServiceFixture _fixture) : IClassFixture<EventServiceFixture>
{
    [Fact]
    [Trait("Event", "Commands")]
    public void CreateEvent_ValidData_ReturnsEvent()
    {
        // Arrange
        var createEventDTO = new CreateEvent(
            Title: "Event: hiking",
            Description: "Info about event",
            StartAt: new DateTime(2026, 5, 2),
            EndAt: new DateTime(2026, 5, 3),
            TotalSeats: 100
        );

        // Act
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var result = eventService.CreateEvent(createEventDTO);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(createEventDTO.Title, result.Title);
        Assert.Equal(createEventDTO.Description, result.Description);
        Assert.Equal(createEventDTO.StartAt, result.StartAt);
        Assert.Equal(createEventDTO.EndAt, result.EndAt);
        Assert.Equal(createEventDTO.TotalSeats, result.TotalSeats);
        Assert.Equal(createEventDTO.TotalSeats, result.AvailableSeats); // если есть
    }

    [Fact]
    [Trait("Event", "Commands")]
    public void UpdateEvent_WithValidData_ReturnUpdatedSameEvent()
    {
        // Arrange
        Guid eventGuid = Guid.NewGuid();

        var original = Event.Create(title: "Test event",
                                    startAt: DateTime.UtcNow.AddDays(3),
                                    endAt: DateTime.UtcNow.AddDays(4),
                                    default,
                                    totalSeats: 100);
        Guid eventId = original.Id;

        Event updated = Event.Create(title: "Updated test event",
                                     startAt: DateTime.UtcNow.AddDays(3),
                                     endAt: DateTime.UtcNow.AddDays(4),
                                     description: "Added description",
                                     totalSeats: 100);

        EventDTO updatedEventDTO = updated.toDTO();

        // Act 
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        eventService.UpdateEventAsync(eventGuid, updatedEventDTO);

        // Assert (подсчет не вызовов методов репозитория Event, а любых обращений к нему)
        Assert.Equal(original.Title, updatedEventDTO.Title);
        Assert.Equal(original.Description, updatedEventDTO.Description);
    }

    [Fact]
    [Trait("Event", "Commands")]
    public async Task DeleteEvent_ByGuidId_WithoutReturns()
    {
        // Arrange
        var createEventDTO = new CreateEvent(Title: "Event: hiking",
                                             Description: "Info about event",
                                             StartAt: new DateTime(2026, 5, 2),
                                             EndAt: new DateTime(2026, 5, 3),
                                             TotalSeats: 100);

        // Act
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var createdEvent = eventService.CreateEvent(createEventDTO);
        await eventService.DeleteEventAsync(createdEvent.Id);

        // Assert
        await eventService.DeleteEventAsync(createdEvent.Id);

        // Assert
        await Assert.ThrowsAsync<EventNotFoundException>(
            () => eventService.GetEventByIdAsync(createdEvent.Id)
        );
    }
}
