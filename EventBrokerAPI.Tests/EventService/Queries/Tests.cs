using Entities.Domain.Models;
using EventBrokerAPI.Tests.Fixture.EventService;
using Shared.DTO;
using Shared.RequestSpecification;
using Shared.ModelExtensions;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Service;

namespace EventBrokerAPI.Tests.EventService.Queries;
public class Tests(Fixture.EventService.Fixture _fixture) : IClassFixture<Fixture.EventService.Fixture>
{
    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetAllEvents_WithValidParameters_ShouldReturnEvents()
    {
        // Arrange
        _fixture.RecreateDatabase();

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();

        var eventParameters = new EventParameters
        {
            Page = 1,
            PageSize = 10
        };

        List<CreateEvent> eventDTOs = [
             new CreateEvent(Title: "Event 1", 
                                       Description: "Info about event",
                                       StartAt: DateTime.UtcNow,
                                       EndAt: DateTime.UtcNow.AddDays(1),
                                       TotalSeats: 4),
             new CreateEvent(Title: "Event 2", 
                                       Description: "Info about event",
                                       StartAt: DateTime.UtcNow,
                                       EndAt: DateTime.UtcNow.AddDays(1),
                                       TotalSeats: 5),
        ];

        // Act
        eventDTOs.ForEach(async dto => await eventService.CreateEventAsync(dto));

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
        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
 
        // Act 
        var created = await eventService.CreateEventAsync(new CreateEvent(Title: "Event 1", Description: "Description 1", StartAt: DateTime.UtcNow.AddDays(1), EndAt: DateTime.UtcNow.AddDays(2), TotalSeats: 100));
        var result = await eventService.GetEventByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
    }



    [Fact]
    [Trait("Event", "Queries")]
    public async Task GetEvents_FilterByTitle_ReturnsMatchingEvents()
    {
        // Arrange
        _fixture.RecreateDatabase();

        const string searchTitle = "hiking";

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
            await eventService.CreateEventAsync(new CreateEvent(Title: "Hiking trip", Description: default, StartAt: DateTime.UtcNow, EndAt: DateTime.UtcNow.AddDays(1), TotalSeats: 10));
            await eventService.CreateEventAsync(new CreateEvent(Title: "Conference", Description: default, StartAt: DateTime.UtcNow, EndAt: DateTime.UtcNow.AddDays(1), TotalSeats: 10));
            await eventService.CreateEventAsync(new CreateEvent(Title: "hiking festival", Description: default, StartAt: DateTime.UtcNow, EndAt: DateTime.UtcNow.AddDays(1), TotalSeats: 10));
      
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
           await eventService.CreateEventAsync(new CreateEvent(Title: "A", Description: default, StartAt: DateTime.UtcNow.AddDays(1), EndAt: DateTime.UtcNow.AddDays(2), TotalSeats: 10));
           await eventService.CreateEventAsync(new CreateEvent(Title: "B", Description: default, StartAt: DateTime.UtcNow.AddDays(3), EndAt: DateTime.UtcNow.AddDays(4), TotalSeats: 10));
           await eventService.CreateEventAsync(new CreateEvent(Title: "C", Description: default, StartAt: DateTime.UtcNow.AddDays(5), EndAt: DateTime.UtcNow.AddDays(6), TotalSeats: 10));

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
        _fixture.RecreateDatabase();

        var eventService = _fixture.serviceProvider.GetRequiredService<IEventService>();
        var eventDTOs = Enumerable.Range(1, 25)
            .Select(i => new CreateEvent(Title: $"Event {i}",
                                         Description: default,
                                         StartAt: DateTime.UtcNow.AddDays(i),
                                         EndAt: DateTime.UtcNow.AddDays(i + 1),
                                         TotalSeats: 10))
            .ToList();

        eventDTOs.ForEach(async dto => await eventService.CreateEventAsync(dto));

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
           await eventService.CreateEventAsync(new CreateEvent(Title: "Hiking", Description: default, StartAt: baseDate.AddDays(1), EndAt: baseDate.AddDays(2), TotalSeats: 10));
           await eventService.CreateEventAsync(new CreateEvent(Title: "Hiking Special", Description: default, StartAt: baseDate.AddDays(10), EndAt: baseDate.AddDays(11), TotalSeats: 10));
           await eventService.CreateEventAsync(new CreateEvent(Title: "Conference", Description: default, StartAt: baseDate.AddDays(1), EndAt: baseDate.AddDays(2), TotalSeats: 10));

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
