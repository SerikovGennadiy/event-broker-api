using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using EventBrokerAPI.Tests.Fixture.EventService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Moq;
using Shared.DTO;
using Shared.ModelExtensions;

namespace EventBrokerAPI.Tests.EventService.Commands;

public class Tests(Fixture.EventService.Fixture _fixture) : IClassFixture<Fixture.EventService.Fixture>
{
    [Fact]
    [Trait("Event", "Commands")]
    public async Task CreateEvent_ValidData_ReturnsEvent()
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
        var result = await eventService.CreateEventAsync(createEventDTO);

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
    public async Task UpdateEvent_WithValidData_ReturnUpdatedSameEvent()
    {
        // Arrange
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();

        var createdDTO = new CreateEvent(
            Title: "Test event",
            Description: "Initial description",
            StartAt: DateTime.UtcNow.AddDays(3),
            EndAt: DateTime.UtcNow.AddDays(4),
            TotalSeats: 100
        );

        var created = await eventService.CreateEventAsync(createdDTO);
        var createdId = created.Id;

        var updatedEventDTO = new EventDTO(
            Title: "Updated test event",
            Description: "Updated description",
            StartAt: DateTime.UtcNow.AddDays(5),
            EndAt: DateTime.UtcNow.AddDays(6),
            TotalSeats: 150
        );

        // Act 
        await eventService.UpdateEventAsync(createdId, updatedEventDTO);
        var updated = await eventService.GetEventByIdAsync(createdId);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(updatedEventDTO.Title, updated.Title);
        Assert.Equal(updatedEventDTO.Description, updated.Description);
        Assert.Equal(updatedEventDTO.StartAt, updated.StartAt);
        Assert.Equal(updatedEventDTO.EndAt, updated.EndAt);
        Assert.Equal(updatedEventDTO.TotalSeats, updated.TotalSeats);
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
        var createdEvent = await eventService.CreateEventAsync(createEventDTO);
        await eventService.DeleteEventAsync(createdEvent.Id);

        // Assert
        await Assert.ThrowsAsync<EventNotFoundException>(
            () => eventService.GetEventByIdAsync(createdEvent.Id)
        );
    }
}
