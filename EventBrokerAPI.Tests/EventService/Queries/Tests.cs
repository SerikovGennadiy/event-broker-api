using Entities.Domain.Models;
using EventBrokerAPI.Tests.Fixture.EventService;
using Moq;
using Shared.DTO;
using Shared.RequestSpecification;
using Shared.ModelExtensions;

namespace EventBrokerAPI.Tests.EventService.Queries;
public class Tests : IClassFixture<EventServiceFixture>
{
    private readonly EventServiceFixture _fixture;
    public Tests(EventServiceFixture fixture)
    {
        _fixture = fixture;
    }

    private void ResetCallCounters()
    {
        _fixture.RepositoryManagerMock.Invocations.Clear();
        _fixture.EventRepositoryMock.Invocations.Clear();
        _fixture.MapperMock.Invocations.Clear();
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
                    Description = "Description 1",
                    TotalSeats = default
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Event 2",
                    StartAt = DateTime.UtcNow.AddDays(3),
                    EndAt = DateTime.UtcNow.AddDays(4),
                    Description = "Description 2",
                    TotalSeats = default
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
        Event @event = new() { Id = eventGuid, Title = "Event 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default };
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
    [Trait("Event", "Queries")]
    public void GetEvents_FilterByTitle_ReturnsMatchingEvents()
    {
        // Arrange
        ResetCallCounters();
        const string searchTitle = "hiking";
        var events = new List<Event>
        {
            new() { Id = Guid.NewGuid(), Title = "Hiking trip", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default },
            new() { Id = Guid.NewGuid(), Title = "Conference", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default },
            new() { Id = Guid.NewGuid(), Title = "hiking festival", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = default }
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
            new() { Id = Guid.NewGuid(), Title = "A", StartAt = now.AddDays(1), EndAt = now.AddDays(2), TotalSeats = default },
            new() { Id = Guid.NewGuid(), Title = "B", StartAt = now.AddDays(3), EndAt = now.AddDays(4), TotalSeats = default },
            new() { Id = Guid.NewGuid(), Title = "C", StartAt = now.AddDays(5), EndAt = now.AddDays(6), TotalSeats = default }
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
                EndAt = DateTime.UtcNow.AddDays(i + 1),
                TotalSeats = default,
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
            new() { Id = Guid.NewGuid(), Title = "Hiking", StartAt = baseDate.AddDays(1), EndAt = baseDate.AddDays(2), TotalSeats = default },
            new() { Id = Guid.NewGuid(), Title = "Hiking Special", StartAt = baseDate.AddDays(10), EndAt = baseDate.AddDays(11), TotalSeats = default },
            new() { Id = Guid.NewGuid(), Title = "Conference", StartAt = baseDate.AddDays(1), EndAt = baseDate.AddDays(2), TotalSeats = default }
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
}
