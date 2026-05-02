using Entities.Domain.Models;
using EventBrokerAPI.Tests.Fixture.EventService;
using Moq;
using Shared.DTO;
using Shared.ModelExtensions;

namespace EventBrokerAPI.Tests.EventService.Commands;

public class Tests : IClassFixture<EventServiceFixture>
{
    private readonly EventServiceFixture _fixture;
    public Tests(EventServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Event", "Commands")]
    public void CreateEvent_ValidData_ReturnsEvent()
    {
        //Arrange
        var createdEventGuid = Guid.CreateVersion7();
        var eventDTO = new EventDTO(Id: Guid.Empty,
                                    Title: "Event: hiking",
                                    Description: "Info about event",
                                    StartAt: new(2026, 5, 2),
                                    EndAt: new(2026, 5, 3));

        var eventEntity = new Event()
        {
            Id = Guid.Empty,
            Title = "Event: hiking",
            Description = "Info about event",
            StartAt = new(2026, 5, 2),
            EndAt = new(2026, 5, 3)
        };

        var createdEventDTO = eventDTO with { Id = createdEventGuid };

        _fixture.MapperMock.Setup(m => m.Map<Event>(eventDTO)).Returns(eventEntity);
        _fixture.MapperMock.Setup(m => m.Map<EventDTO>(It.IsAny<Event>())).Returns(createdEventDTO);
        _fixture.EventRepositoryMock.Setup(repo => repo.CreateEvent(It.IsAny<Event>())).Verifiable();

        // Act
        var result = _fixture.EventService.CreateEvent(eventDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEventDTO, result);
        _fixture.RepositoryManagerMock.Verify(rm => rm.Event.CreateEvent(It.IsAny<Event>()), Times.Once());
    }

    [Fact]
    [Trait("Event", "Commands")]
    public void UpdateEvent_WithValidData_ReturnUpdatedSameEvent()
    {
        // Arrange
        Guid eventGuid = Guid.CreateVersion7();
        Event @event = new Event()
        {
            Id = eventGuid,
            Title = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(2)
        };

        Event updatedEvent = new Event()
        {
            Id = eventGuid,
            Title = "Updated test event",
            Description = "Added description",
            StartAt = DateTime.UtcNow.AddDays(3),
            EndAt = DateTime.UtcNow.AddDays(4)
        };
        EventDTO updatedEventDTO = updatedEvent.toDTO();

        _fixture.MapperMock.Setup(m => m.Map<Event>(updatedEventDTO)).Returns(updatedEvent);
        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventGuid)).Returns(@event);

        // Act 
        _fixture.EventService.UpdateEvent(eventGuid, updatedEventDTO);

        // Assert (подсчет не вызовов методов репозитория Event, а любых обращений к нему)
        _fixture.RepositoryManagerMock.Verify(rm => rm.Event, Times.AtLeastOnce);
    }

    [Fact]
    [Trait("Event", "Commands")]
    public void DeleteEvent_ByGuidId_WithoutReturns()
    {
        // Arrange
        Guid eventGuid = Guid.CreateVersion7();
        Event @event = new Event()
        {
            Id = eventGuid,
            Title = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(2)
        };
        EventDTO eventDTO = @event.toDTO();

        _fixture.MapperMock.Setup(m => m.Map<Event>(eventDTO)).Returns(@event);
        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventGuid)).Returns(@event);

        // Act
        _fixture.EventService.DeleteEvent(eventGuid);

        // Assert
        _fixture.RepositoryManagerMock.Verify(rm => rm.Event.DeleteEvent(It.IsAny<Event>()), Times.Once());
    }
}
