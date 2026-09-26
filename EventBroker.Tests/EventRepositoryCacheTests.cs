using Events.Domain.Models;
using Moq;
using StackExchange.Redis;
using System.Text.Json;
using Xunit;

namespace Events.Tests;

/// <summary>Cache-Aside в EventRepository: попадания, промахи, топ-10, инвалидация.</summary>
public sealed class EventRepositoryCacheTests(EventsFixture fixture) : IClassFixture<EventsFixture>
{
    [Fact(DisplayName = "Попадание в кеш: репозиторий (БД) не вызывается")]
    public async Task GetById_CacheHit_DoesNotTouchDatabase()
    {
        // Arrange: БД ПУСТА — если репозиторий обратится к ней, сущности там не будет.
        using var context = fixture.CreateDbContext();
        var cache = fixture.CreateCache();
        var sut = fixture.CreateRepository(context, cache.Mock);

        var expected = EventsFixture.CreateEvent("Концерт", totalSeats: 100, sold: 20);
        cache.Store[EventsFixture.EventKey(expected.Id)] =
            (JsonSerializer.Serialize(expected, EventsFixture.JsonOptions), TimeSpan.FromMinutes(5));

        // Act
        var actual = await sut.GetByIdAsync(expected.Id);

        // Assert: данные только из кеша — в пустой БД их нет, значит чтения БД не было.
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal("Концерт", actual.Title);
        Assert.Equal(80, actual.AvailableSeats);
        Assert.Empty(context.Events);
        cache.Mock.Verify(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact(DisplayName = "Промах: данные из БД и прогрев кеша с TTL")]
    public async Task GetById_CacheMiss_LoadsFromDb_AndWarmsCache()
    {
        // Arrange
        using var context = fixture.CreateDbContext();
        var entity = EventsFixture.CreateEvent("Спектакль", totalSeats: 50, sold: 5);
        context.Events.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear(); // эмулируем новый scope: трекера нет, пойдём в БД

        var cache = fixture.CreateCache();
        var sut = fixture.CreateRepository(context, cache.Mock);

        // Act
        var actual = await sut.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(entity.Id, actual.Id);
        Assert.Equal(45, actual.AvailableSeats);

        var key = EventsFixture.EventKey(entity.Id);
        Assert.True(cache.Store.ContainsKey(key));
        Assert.Equal((Expiration)TimeSpan.FromMinutes(5), cache.Store[key].Ttl);
    }

    [Fact(DisplayName = "Топ-10: упорядочен по проценту продаж и кешируется")]
    public async Task GetTopSelling_CacheMiss_ReturnsOrdered_AndCaches()
    {
        // Arrange: 90%, 50%, 10% проданных мест.
        using var context = fixture.CreateDbContext();
        var low = EventsFixture.CreateEvent("Низкий спрос", totalSeats: 100, sold: 10);
        var high = EventsFixture.CreateEvent("Хит продаж", totalSeats: 100, sold: 90);
        var mid = EventsFixture.CreateEvent("Середняк", totalSeats: 200, sold: 100);
        context.Events.AddRange(low, high, mid);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var cache = fixture.CreateCache();
        var sut = fixture.CreateRepository(context, cache.Mock);

        // Act
        var actual = (await sut.GetTop10SellingEventsAsync()).ToList();

        // Assert
        Assert.Equal([high.Id, mid.Id, low.Id], actual.Select(e => e.Id));
        Assert.True(cache.Store.ContainsKey(EventsFixture.TopKey));
        Assert.Equal((Expiration)TimeSpan.FromMinutes(1), cache.Store[EventsFixture.TopKey].Ttl);
    }

    [Fact(DisplayName = "Топ-10 из кеша: репозиторий (БД) не вызывается")]
    public async Task GetTopSelling_CacheHit_DoesNotTouchDatabase()
    {
        // Arrange: БД ПУСТА, топ заранее прогрет.
        using var context = fixture.CreateDbContext();
        var cached = new List<Events.Domain.Models.Event>
        {
            EventsFixture.CreateEvent("Из кеша", totalSeats: 10, sold: 9),
        };
        var cache = fixture.CreateCache();
        cache.Store[EventsFixture.TopKey] =
            (JsonSerializer.Serialize(cached, EventsFixture.JsonOptions), TimeSpan.FromMinutes(1));

        var sut = fixture.CreateRepository(context, cache.Mock);

        // Act
        var actual = (await sut.GetTop10SellingEventsAsync()).ToList();

        // Assert
        Assert.Single(actual);
        Assert.Equal("Из кеша", actual[0].Title);
        Assert.Equal(1, actual[0].AvailableSeats);
        Assert.Empty(context.Events);
    }

    [Fact(DisplayName = "Инвалидация: удаляется event:{id}, events:top10 живёт на TTL")]
    public async Task InvalidateEventCache_RemovesEventKey_KeepsTopKey()
    {
        // Arrange: оба ключа прогреты (стратегия — инвалидация только события).
        using var context = fixture.CreateDbContext();
        var cache = fixture.CreateCache();
        var eventId = Guid.NewGuid();
        cache.Store[EventsFixture.EventKey(eventId)] = ("{}", default);
        cache.Store[EventsFixture.TopKey] = ("[]", default);

        var sut = fixture.CreateRepository(context, cache.Mock);

        // Act
        await sut.InvalidateEventCacheAsync(eventId);

        // Assert
        Assert.False(cache.Store.ContainsKey(EventsFixture.EventKey(eventId)));
        Assert.True(cache.Store.ContainsKey(EventsFixture.TopKey));
        Assert.Contains(EventsFixture.EventKey(eventId), cache.DeletedKeys);
        Assert.DoesNotContain(EventsFixture.TopKey, cache.DeletedKeys);
    }

    [Fact(DisplayName = "Битая запись в кеше: откат к БД с перезаписью")]
    public async Task GetById_CorruptCache_FallsBackToDb()
    {
        // Arrange
        using var context = fixture.CreateDbContext();
        var entity = EventsFixture.CreateEvent("Лекция", totalSeats: 30, sold: 3);
        context.Events.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var cache = fixture.CreateCache();
        cache.Store[EventsFixture.EventKey(entity.Id)] = ("{ not-json", default);

        var sut = fixture.CreateRepository(context, cache.Mock);

        // Act
        var actual = await sut.GetByIdAsync(entity.Id);

        // Assert: вернулись данные БД, кеш перезаписан валидным JSON.
        Assert.NotNull(actual);
        Assert.Equal("Лекция", actual.Title);
        var rewritten = cache.Store[EventsFixture.EventKey(entity.Id)].Value;
        Assert.Equal(entity.Id, JsonSerializer.Deserialize<Events.Domain.Models.Event>(rewritten, EventsFixture.JsonOptions)!.Id);
    }
}
