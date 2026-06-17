using Entities.Domain.Models;
using EventBrokerAPI.Tests.Fixture.EventService;
using Shared.DTO;
using Shared.RequestSpecification;
using Shared.ModelExtensions;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Service;

namespace EventBrokerAPI.Tests.EventService.Queries;
public class Tests(EventServiceFixture _fixture) : IClassFixture<EventServiceFixture>
{
    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetAllEvents_WithValidParameters_ShouldReturnEvents()
    {
        // Arrange
        var eventParameters = new EventParameters
        {
            Page = 1,
            PageSize = 10
        };
        Event updated = Event.Create(title: "Event 1",
                                     startAt: DateTime.UtcNow.AddDays(1),
                                     endAt: DateTime.UtcNow.AddDays(2),
                                     description: "Description 1",
                                     totalSeats: 100);

        EventDTO updatedEventDTO = updated.toDTO();
        List<Event> events = [
               Event.Create(title: "Event 1",
                            startAt: DateTime.UtcNow.AddDays(1),
                            endAt: DateTime.UtcNow.AddDays(2),
                            description: "Description 1",
                            totalSeats: 100),
               Event.Create(title: "Event 2",
                           startAt: DateTime.UtcNow.AddDays(3),
                           endAt: DateTime.UtcNow.AddDays(4),
                           description: "Description 2",
                           totalSeats: 100),
        ];

        // Act
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();

        var dtos = events.Select(x => new CreateEvent(x.Title, x.Description, x.StartAt, x.EndAt, x.TotalSeats)).ToList();
        dtos.ForEach(dto => eventService.CreateEvent(dto));

        var (resultDTOs, pageData) = await eventService.GetAllEventsAsync(eventParameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Equal(2, resultDTOs.Count());
        Assert.NotNull(pageData);
        Assert.Equal(1, pageData.CurrentPageNumber);
    }

    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetEvent_GuidId_ReturnEvent()
    {
        // Arrage
        var eventGuid = Guid.NewGuid();
        Event @event = Event.Create(title: "Event 1",
                                    startAt: DateTime.UtcNow.AddDays(1),
                                    endAt: DateTime.UtcNow.AddDays(2),
                                    description: "Description 1",
                                    totalSeats: 100);

        // Act 
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var created = eventService.CreateEvent(new CreateEvent(Title: "Event 1", Description: "Description 1", StartAt: DateTime.UtcNow.AddDays(1), EndAt: DateTime.UtcNow.AddDays(2), TotalSeats: 100));
        var result = await eventService.GetEventByIdAsync(eventGuid);

        // Assert
        Assert.NotNull(result);
    }



    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetEvents_FilterByTitle_ReturnsMatchingEvents()
    {
        // Arrange
        const string searchTitle = "hiking";

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
            eventService.CreateEvent(new CreateEvent(Title: "Hiking trip", Description: default, StartAt: DateTime.UtcNow, EndAt: DateTime.UtcNow.AddDays(1), TotalSeats: default));
            eventService.CreateEvent(new CreateEvent(Title: "Conference", Description: default, StartAt: DateTime.UtcNow, EndAt: DateTime.UtcNow.AddDays(1), TotalSeats: default));
            eventService.CreateEvent(new CreateEvent(Title: "hiking festival", Description: default, StartAt: DateTime.UtcNow, EndAt: DateTime.UtcNow.AddDays(1), TotalSeats: default));
      
        var parameters = new EventParameters { Page = 1, PageSize = 10, Title = searchTitle };

        // Act
        var (resultDTOs, pageData) = await eventService.GetAllEventsAsync(parameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Equal(2, resultDTOs.Count());
        Assert.All(resultDTOs, e => Assert.Contains(searchTitle, e.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetEvents_FilterByDateRange_ReturnsEventsWithinRange()
    {
        // Arrange
        var now = DateTime.UtcNow.Date;
        var from = now.AddDays(2);
        var to = now.AddDays(5);

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
            eventService.CreateEvent(new CreateEvent(Title: "A", Description: default, StartAt: DateTime.UtcNow.AddDays(1), EndAt: DateTime.UtcNow.AddDays(2), TotalSeats: default));
            eventService.CreateEvent(new CreateEvent(Title: "B", Description: default, StartAt: DateTime.UtcNow.AddDays(3), EndAt: DateTime.UtcNow.AddDays(4), TotalSeats: default));
            eventService.CreateEvent(new CreateEvent(Title: "C", Description: default, StartAt: DateTime.UtcNow.AddDays(5), EndAt: DateTime.UtcNow.AddDays(6), TotalSeats: default));

        var parameters = new EventParameters { Page = 1, PageSize = 10, From = from, To = to };

        // Act
        var (resultDTOs, pageData) = await eventService.GetAllEventsAsync(parameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Single(resultDTOs);
        Assert.Equal("B", resultDTOs.First().Title);
    }

    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetEvents_Pagination_WorksCorrectly()
    {
        // Arrange
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var eventDTOs = Enumerable.Range(1, 25)
            .Select(i => new CreateEvent(Title: $"Event {i}",
                                         Description: default,
                                         StartAt: DateTime.UtcNow.AddDays(i),
                                         EndAt: DateTime.UtcNow.AddDays(i + 1),
                                         TotalSeats: default))
            .ToList();

        eventDTOs.ForEach(dto => eventService.CreateEvent(dto));

        var page1 = new EventParameters { Page = 1, PageSize = 10 };
        var page3 = new EventParameters { Page = 3, PageSize = 10 };

        // Act
        var (eventsPage1, pageData1) = await eventService.GetAllEventsAsync(page1);
        var (eventsPage3, pageData3) = await eventService.GetAllEventsAsync(page3);

        // Assert
        Assert.Equal(10, eventsPage1.Count());
        Assert.Equal(5, eventsPage3.Count());
    }

    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetEvents_CombinedFilter_TitleAndRange_ReturnsExpected()
    {
        // Arrange
        var baseDate = DateTime.UtcNow.Date;

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        eventService.CreateEvent(new CreateEvent(Title: "Hiking", Description: default, StartAt: baseDate.AddDays(1), EndAt: baseDate.AddDays(2), TotalSeats: default));
        eventService.CreateEvent(new CreateEvent(Title: "Hiking Special", Description: default, StartAt: baseDate.AddDays(10), EndAt: baseDate.AddDays(11), TotalSeats: default));
        eventService.CreateEvent(new CreateEvent(Title: "Conference", Description: default, StartAt: baseDate.AddDays(1), EndAt: baseDate.AddDays(2), TotalSeats: default));

        // Данные для фильтрации по всем параметрам
        var parameters = new EventParameters
        {
            Page = 1,
            PageSize = 10,
            Title = "hiking",
            From = baseDate,
            To = baseDate.AddDays(5)
        };

        // Act
        var (resultDTOs, pageData) = await eventService.GetAllEventsAsync(parameters);

        // Assert
        Assert.NotNull(resultDTOs);
        Assert.Single(resultDTOs);
        Assert.Equal("Hiking", resultDTOs.First().Title);
    }
}
