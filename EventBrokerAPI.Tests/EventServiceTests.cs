using AutoMapper;
using Contracts.Repository;
using Contracts.Service;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
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

    [Fact]
    [Trait("Event", "Queries")]
    public void GetEvents_Pagination_WorksCorrectly()
    {
        // Arrange
        ResetCallCounters();

        var events = Enumerable.Range(1, 25)
            .Select(i => new Event
            {
                Id = Guid.NewGuid(),
                Title = $"Event {i}",
                StartAt = DateTime.UtcNow.AddDays(i),
                EndAt = DateTime.UtcNow.AddDays(i + 1)
            })
            .ToList();

        _fixture.EventRepositoryMock
            .Setup(r => r.GetAllEvents(It.IsAny<EventParameters>()))
            .Returns((EventParameters p) => PaginatedList<Event>.ToPagedList(events, p.Page, p.PageSize));
        
        _fixture.MapperMock.Setup(m => m.Map<IEnumerable<EventDTO>>(It.IsAny<IEnumerable<Event>>()))
            .Returns((IEnumerable<Event> evs) => evs.Select(e => e.toDTO()).ToList());

        var page1 = new EventParameters { Page = 1, PageSize = 10 };
        var page3 = new EventParameters { Page = 3, PageSize = 10 };

        // Act
        var (eventsPage1, pageData1) = _fixture.EventService.GetAllEvents(page1);
        var (eventsPage3, pageData3) = _fixture.EventService.GetAllEvents(page3);  
        
        // Assert
        Assert.Equal(10, eventsPage1.Count());
        Assert.Equal(5, eventsPage3.Count());
        _fixture.EventRepositoryMock.Verify(r => r.GetAllEvents(It.IsAny<EventParameters>()), Times.Exactly(2));
    }

    [Fact]
    [Trait("Event", "Queries")]
    public void GetEvents_CombinedFilter_TitleAndRange_ReturnsExpected()
    {
        // Arrange
        ResetCallCounters();

        var baseDate = DateTime.UtcNow.Date;
        var events = new List<Event>
        {
            new() { Id = Guid.NewGuid(), Title = "Hiking", StartAt = baseDate.AddDays(1), EndAt = baseDate.AddDays(2) },
            new() { Id = Guid.NewGuid(), Title = "Hiking Special", StartAt = baseDate.AddDays(10), EndAt = baseDate.AddDays(11) },
            new() { Id = Guid.NewGuid(), Title = "Conference", StartAt = baseDate.AddDays(1), EndAt = baseDate.AddDays(2) }
        };

        // Данные для фильтрации по всем параметрам
        var parameters = new EventParameters
        {
            Page = 1,
            PageSize = 10,
            Title = "hiking",
            From = baseDate,
            To = baseDate.AddDays(5)
        };

        // Локальная фильтрация для вычисления ожидаемого результата
        var expectedEvents = events
            .Where(e => e.Title.Contains(parameters.Title, StringComparison.OrdinalIgnoreCase)
                     && e.StartAt >= parameters.From
                     && e.EndAt <= parameters.To)
            .ToList();

        var expectedPaginatedEvents = PaginatedList<Event>.ToPagedList(expectedEvents, pageNumber: parameters.Page, pageSize: parameters.PageSize);

        // Настройка моков
        _fixture.EventRepositoryMock
            .Setup(r => r.GetAllEvents(It.Is<EventParameters>(p =>
                p.Title == parameters.Title
                && p.From == parameters.From
                && p.To == parameters.To
                && p.Page == parameters.Page
                && p.PageSize == parameters.PageSize)))
            .Returns(expectedPaginatedEvents);

        _fixture.MapperMock
            .Setup(m => m.Map<IEnumerable<EventDTO>>(It.IsAny<IEnumerable<Event>>()))
            .Returns((IEnumerable<Event> evs) => evs.Select(e => e.toDTO()).ToList());



        // Act
        var (resultDTOs, pageData) = _fixture.EventService.GetAllEvents(parameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Single(resultDTOs);
        Assert.Equal("Hiking", resultDTOs.First().Title);
        _fixture.EventRepositoryMock.Verify(r => r.GetAllEvents(It.IsAny<EventParameters>()), Times.Once());
    }

    [Fact]
    [Trait("Event", "Queries")]
    public void GetEvent_GetById_Unsuccessful_ReturnsNull()
    {
        // Arrange
        var unexistingGuid = Guid.CreateVersion7();
        List<Event> events = [
            new () { Id = Guid.CreateVersion7(), Title = "A", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(2) },
            new () { Id = Guid.CreateVersion7(), Title = "B", StartAt = DateTime.UtcNow.AddDays(2), EndAt = DateTime.UtcNow.AddDays(1) },
            new () { Id = Guid.CreateVersion7(), Title = "C", StartAt = DateTime.UtcNow.AddDays(5), EndAt = DateTime.UtcNow.AddDays(1) }
        ];

        _fixture.EventRepositoryMock.Setup(r => r.GetById(unexistingGuid)).Returns((Event?)null);

        // Act
        var exception = Record.Exception(() => _fixture.EventService.GetEventById(unexistingGuid));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<EventNotFoundException>(exception);
    }
}

