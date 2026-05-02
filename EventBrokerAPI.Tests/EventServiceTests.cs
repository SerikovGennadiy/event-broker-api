using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Moq;
using Repository;
using Service;
using Shared.DTO;
using Shared.ModelExtensions;
using Shared.RequestSpecification;
using System.Reflection.Metadata;

namespace EventBrokerAPI.Tests;
public class EventServiceFixture : IDisposable
{
    public EventService EventService { get; }

    public Mock<IRepositoryManager> RepositoryManagerMock { get; } = new();
    public Mock<IEventRepository> EventRepositoryMock { get; } = new();
    public Mock<IMapper> MapperMock { get; } = new();

    public EventServiceFixture()
    {
        RepositoryManagerMock.Setup(x => x.Event)
            .Returns(EventRepositoryMock.Object);

        EventService = new EventService(
            RepositoryManagerMock.Object,
            MapperMock.Object
        );
    }

    public void Dispose()
    {
        RepositoryManagerMock.Reset();
        EventRepositoryMock.Reset();
        MapperMock.Reset();

        GC.SuppressFinalize(this);
    }
}

public class EventServiceTests : IClassFixture<EventServiceFixture>
{
    private readonly EventServiceFixture _fixture;
    public EventServiceTests(EventServiceFixture fixture) { 
        _fixture = fixture;
    }

    private void ResetCallCounters()
    {
        _fixture.RepositoryManagerMock.Invocations.Clear();
        _fixture.EventRepositoryMock.Invocations.Clear();
        _fixture.MapperMock.Invocations.Clear();
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
    [Trait("Event", "Queries")]
    public void GetAllEvents_WithValidParameters_ShouldReturnEvents()
    {
        // Arrange
        var eventParameters = new EventParameters
        {
            Page = 1,
            PageSize = 10
        };

        List<Event> events = [
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 1",
                    StartAt = DateTime.UtcNow.AddDays(1),
                    EndAt = DateTime.UtcNow.AddDays(2),
                    Description = "Description 1"
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 2",
                    StartAt = DateTime.UtcNow.AddDays(3),
                    EndAt = DateTime.UtcNow.AddDays(4),
                    Description = "Description 2"
                }
        ];

        var eventDTOs = events.Select(e => e.toDTO()).ToList();
        var paginatedList = PaginatedList<Event>.ToPagedList(events, eventParameters.Page, eventParameters.PageSize);

        _fixture.EventRepositoryMock.Setup(r => r.GetAllEvents(eventParameters)).Returns(paginatedList);
        _fixture.MapperMock.Setup(m => m.Map<IEnumerable<EventDTO>>(It.IsAny<IEnumerable<Event>>())).Returns(eventDTOs);

        // Act
        var (resultDTOs, pageData) = _fixture.EventService.GetAllEvents(eventParameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Equal(2, resultDTOs.Count());
        Assert.NotNull(pageData);
        Assert.Equal(1, pageData.CurrentPageNumber);
    }

    [Fact]
    [Trait("Event", "Queries")]
    public void GetEvent_GuidId_ReturnEvent()
    {
        // Arrage
        ResetCallCounters();

        var eventGuid = Guid.CreateVersion7();
        Event @event = new() { Id = eventGuid, Title = "Event 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) };
        EventDTO eventDTO = @event.toDTO();

        _fixture.EventRepositoryMock.Setup(r => r.GetById(eventGuid)).Returns(@event);
        _fixture.MapperMock.Setup(m => m.Map<EventDTO>(@event)).Returns(eventDTO);

        // Act 
        var result = _fixture.EventService.GetEventById(eventGuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventDTO, result);
        _fixture.RepositoryManagerMock.Verify(rm => rm.Event.GetById(It.IsAny<Guid>()), Times.Once());
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

    [Fact]
    [Trait("Event", "Queries")]
    public void GetEvents_FilterByTitle_ReturnsMatchingEvents()
    {
        // Arrange
        ResetCallCounters();
        const string searchTitle = "hiking";
        var events = new List<Event>
        {
            new() { Id = Guid.NewGuid(), Title = "Hiking trip", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Conference", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "hiking festival", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) }
        };

        var filteredEvents = events
            .Where(e => e.Title.Contains(searchTitle, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var filteredEventDTOs = filteredEvents.Select(e => e.toDTO()).ToList();

        var paginatedFiltered = PaginatedList<Event>.ToPagedList(filteredEvents, pageNumber: 1, pageSize: 10);

        _fixture.EventRepositoryMock
            .Setup(r => r.GetAllEvents(It.Is<EventParameters>(p =>
                p.Title == searchTitle && p.Page == 1 && p.PageSize == 10)))
            .Returns(paginatedFiltered);

        _fixture.MapperMock
            .Setup(m => m.Map<IEnumerable<EventDTO>>(It.IsAny<IEnumerable<Event>>()))
            .Returns(filteredEventDTOs);

        var parameters = new EventParameters { Page = 1, PageSize = 10, Title = searchTitle };

        // Act
        var (resultDTOs, pageData) = _fixture.EventService.GetAllEvents(parameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Equal(filteredEvents.Count, resultDTOs.Count());
        Assert.All(resultDTOs, e => Assert.Contains(searchTitle, e.Title, StringComparison.OrdinalIgnoreCase));
        _fixture.EventRepositoryMock.Verify(r => r.GetAllEvents(It.IsAny<EventParameters>()), Times.Once());
    }

    [Fact]
    [Trait("Event", "Queries")]
    public void GetEvents_FilterByDateRange_ReturnsEventsWithinRange()
    {
        // Arrange
        ResetCallCounters();

        var now = DateTime.UtcNow.Date;
        var from = now.AddDays(2);
        var to = now.AddDays(5);

        var events = new List<Event>
        {
            new() { Id = Guid.NewGuid(), Title = "A", StartAt = now.AddDays(1), EndAt = now.AddDays(2) },
            new() { Id = Guid.NewGuid(), Title = "B", StartAt = now.AddDays(3), EndAt = now.AddDays(4) },
            new() { Id = Guid.NewGuid(), Title = "C", StartAt = now.AddDays(5), EndAt = now.AddDays(6) }
        };

        var filteredEvents = events
            .Where(e => e.StartAt >= from && e.EndAt <= to)
            .ToList();

        var filteredEventDTOs = filteredEvents.Select(e => e.toDTO()).ToList();

        var paginatedResult = PaginatedList<Event>.ToPagedList(filteredEvents, pageNumber: 1, pageSize: 10);

        var parameters = new EventParameters { Page = 1, PageSize = 10, From = from, To = to };

        _fixture.EventRepositoryMock
            .Setup(r => r.GetAllEvents(It.Is<EventParameters>(p =>
                p.From == from && p.To == to && p.Page == 1 && p.PageSize == 10)))
            .Returns(paginatedResult);

        _fixture.MapperMock
            .Setup(m => m.Map<IEnumerable<EventDTO>>(It.IsAny<IEnumerable<Event>>()))
            .Returns(filteredEventDTOs);

        // Act
        var (resultDTOs, pageData) = _fixture.EventService.GetAllEvents(parameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Single(resultDTOs);
        Assert.Equal("B", resultDTOs.First().Title);
        _fixture.EventRepositoryMock.Verify(r => r.GetAllEvents(It.IsAny<EventParameters>()), Times.Once());
    }
}

