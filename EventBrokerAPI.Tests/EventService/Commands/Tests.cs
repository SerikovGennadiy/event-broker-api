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
        // Arrange
        var createdEventGuid = Guid.NewGuid();

        // Создаем EventInfo без Id
        var createEventDTO = new CreateEvent(
            Title: "Event: hiking",
            Description: "Info about event",
            StartAt: new DateTime(2026, 5, 2),
            EndAt: new DateTime(2026, 5, 3),
            TotalSeats: 100
        );

        // Добавляем Id через with (теперь работает!)
        var @event = new Event()
        {
            Id = createdEventGuid,
            Title = "Event: hiking",
            Description = "Info about event",
            StartAt = new DateTime(2026, 5, 2),
            EndAt = new DateTime(2026, 5, 3),
            TotalSeats = 100
        };

        var eventDTO = new EventInfo(Id: createdEventGuid,
                                     Title: "Event: hiking",
                                     Description: "Info about event",
                                     StartAt: new DateTime(2026, 5, 2),
                                     EndAt: new DateTime(2026, 5, 3),
                                     TotalSeats: 100,
                                     AvailableSeats: 100);

        _fixture.MapperMock.Setup(m => m.Map<Event>(createEventDTO)).Returns(@event);
        _fixture.MapperMock.Setup(m => m.Map<EventInfo>(It.IsAny<Event>())).Returns(eventDTO);
        _fixture.EventRepositoryMock.Setup(repo => repo.CreateEvent(It.IsAny<Event>())).Verifiable();

        // Act
        var result = _fixture.EventService.CreateEvent(createEventDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventDTO, result);
        _fixture.RepositoryManagerMock.Verify(rm => rm.Event.CreateEvent(It.IsAny<Event>()), Times.Once());
    }

    [Fact]
    [Trait("Event", "Commands")]
    public void UpdateEvent_WithValidData_ReturnUpdatedSameEvent()
    {
        // Arrange
        Guid eventGuid = Guid.NewGuid();
        Event @event = new Event()
        {
            Id = eventGuid,
            Title = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 100
        };

        Event updatedEvent = new Event()
        {
            Id = eventGuid,
            Title = "Updated test event",
            Description = "Added description",
            StartAt = DateTime.UtcNow.AddDays(3),
            EndAt = DateTime.UtcNow.AddDays(4),
            TotalSeats = 100
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
        Guid eventGuid = Guid.NewGuid();
        Event @event = new Event()
        {
            Id = eventGuid,
            Title = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 100
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
