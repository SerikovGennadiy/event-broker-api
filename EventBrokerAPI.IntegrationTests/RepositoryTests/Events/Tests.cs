namespace EventBrokerAPI.IntegrationTests.RepositoryTests.Events;

public class Tests(Fixture _fixture) : IClassFixture<Fixture>
{
    [Fact]
    public async Task GetByIdAsync_ReturnsEvent()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);
        var @event = Event.Create(title: "Testing",
                                  startAt: DateTime.UtcNow,
                                  endAt: DateTime.UtcNow.AddDays(1),
                                  description: "integration tests",
                                  totalSeats: 1);

        repo.Event.CreateEvent(@event);
        await repo.SaveAsync();

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var result = await repo.Event.GetByIdAsync(@event.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(@event.Id, result.Id);
        Assert.Equal("Testing", result.Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        var event1 = Event.Create("Event 1", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), null, 50);
        var event2 = Event.Create("Event 2", DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4), null, 100);
        var event3 = Event.Create("Event 3", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), null, 75);
        repo.Event.CreateEvent(event1);
        repo.Event.CreateEvent(event2);
        repo.Event.CreateEvent(event3);
        await repo.SaveAsync();

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);

        var parameters = new EventParameters { Page = 1, PageSize = 10 };
        var result = await repo.Event.GetAllEventsAsync(parameters);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.PageMetaData.EntitiesCountTotal);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        var now = DateTime.UtcNow;
        for (int i = 1; i <= 25; i++)
        {
            var @event = Event.Create($"Event {i:D2}", now.AddDays(i), now.AddDays(i + 1), null, 100);
            repo.Event.CreateEvent(@event);
        }
        await repo.SaveAsync();

        var parameters = new EventParameters { Page = 2, PageSize = 10 };

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var result = await repo.Event.GetAllEventsAsync(parameters);

        // Assert
        Assert.Equal(10, result.Count);
        Assert.Equal(25, result.PageMetaData.EntitiesCountTotal);
        Assert.Equal(3, result.PageMetaData.TotalPages);
        Assert.Equal(2, result.PageMetaData.CurrentPageNumber);
    }

    [Fact]
    public async Task DeleteEvent_ReturnSuccess()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);
        var @event = Event.Create(title: "Testing",
                                startAt: DateTime.UtcNow,
                                endAt: DateTime.UtcNow.AddDays(1),
                                description: "integration tests",
                                totalSeats: 1);

        repo.Event.CreateEvent(@event);
        await repo.SaveAsync();

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        repo.Event.DeleteEvent(@event);
        await repo.SaveAsync();

        var deleted = await repo.Event.GetByIdAsync(@event.Id);

        // Assert
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithTitleFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        var now = DateTime.UtcNow;
        var @event1 = Event.Create("Conference 2024", now.AddDays(1), now.AddDays(2), null, 100);
        var @event2 = Event.Create("Workshop Coding", now.AddDays(3), now.AddDays(4), null, 50);
        var @event3 = Event.Create("Conference Spring", now.AddDays(5), now.AddDays(6), null, 75);

        repo.Event.CreateEvent(@event1);
        repo.Event.CreateEvent(@event2);
        repo.Event.CreateEvent(@event3);
        await repo.SaveAsync();

        var parameters = new EventParameters { Title = "Conference", Page = 1, PageSize = 10 };

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var result = await repo.Event.GetAllEventsAsync(parameters);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, @event => Assert.Contains("Conference", @event.Title));
    }

    [Fact]
    public async Task GetAllEventsAsync_WithDateRangeFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        var now = DateTime.UtcNow;
        var startRange = now.AddDays(5);
        var endRange = now.AddDays(15);

        var @event1 = Event.Create("Event Before", now.AddDays(1), now.AddDays(2), null, 100);
        var @event2 = Event.Create("Event In Range", now.AddDays(10), now.AddDays(11), null, 100);
        var @event3 = Event.Create("Event After", now.AddDays(20), now.AddDays(21), null, 100);

        repo.Event.CreateEvent(@event1);
        repo.Event.CreateEvent(@event2);
        repo.Event.CreateEvent(@event3);
        await repo.SaveAsync();

        var parameters = new EventParameters { From = startRange, To = endRange, Page = 1, PageSize = 10 };

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var result = await repo.Event.GetAllEventsAsync(parameters);

        // Assert
        Assert.Single(result);
        Assert.Equal("Event In Range", result.First().Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithTitleAndDateRangeFilter_ReturnsFilteredEvents()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var arrangeContext = _fixture.CreateTestDbContext();
        var repo = new RepositoryManager(arrangeContext);

        var now = DateTime.UtcNow;
        var startRange = now.AddDays(5);
        var endRange = now.AddDays(15);

        var @event1 = Event.Create("Conference Early", now.AddDays(1), now.AddDays(2), null, 100);
        var @event2 = Event.Create("Conference Scheduled", now.AddDays(10), now.AddDays(11), null, 100);
        var @event3 = Event.Create("Workshop Scheduled", now.AddDays(10), now.AddDays(11), null, 100);
        var @event4 = Event.Create("Conference Late", now.AddDays(20), now.AddDays(21), null, 100);

        repo.Event.CreateEvent(@event1);
        repo.Event.CreateEvent(@event2);
        repo.Event.CreateEvent(@event3);
        repo.Event.CreateEvent(@event4);
        await repo.SaveAsync();

        var parameters = new EventParameters
        {
            Title = "Conference",
            From = startRange,
            To = endRange,
            Page = 1,
            PageSize = 10
        };

        // Act
        await using var actContext = _fixture.CreateTestDbContext();
        repo = new RepositoryManager(actContext);
        var result = await repo.Event.GetAllEventsAsync(parameters);

        // Assert
        Assert.Single(result);
        Assert.Equal("Conference Scheduled", result.First().Title);
    }
}