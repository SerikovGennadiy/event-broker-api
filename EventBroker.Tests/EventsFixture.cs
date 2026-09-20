using AutoMapper;
using Events.Application.Contracts.Persistence;
using Events.Application.Contracts.Services;
using Events.Application.Contracts.Services.Messaging;
using Events.Application.Services;
using Events.Domain.Models;
using Events.Domain.Options;
using Events.Infrastructure.Persistence;
using Events.Infrastructure.Persistence.Repository;
using Messaging.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace Events.Tests;

/// <summary>
/// Общая фикстура unit-тестов Events-сервиса:
/// изолированная InMemory-БД на каждый тест, словарный фейк Redis-кеша,
/// фабрики репозитория и сервиса с подменёнными зависимостями.
/// </summary>
public sealed class EventsFixture
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public RedisSettings CacheSettings { get; } = new()
    {
        EventTtlMinutes = 5,
        TopEventsTtlMinutes = 1,
    };

    /// <summary>Новый изолированный InMemory-контекст (уникальное имя БД на вызов).</summary>
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"events-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Фейк IDatabase на словаре: StringGet/StringSet/KeyDelete работают как Redis,
    /// плюс фиксируют TTL записей и удалённые ключи для assert'ов.
    /// </summary>
    public FakeCache CreateCache()
    {
        var store = new Dictionary<string, (string Value, Expiration Ttl)>();
        var deletedKeys = new List<string>();
        var mock = new Mock<IDatabase>(MockBehavior.Strict);

        mock.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisKey key, CommandFlags _) =>
                store.TryGetValue((string)key!, out var entry) ? (RedisValue)entry.Value : RedisValue.Null);

        mock.Setup(d => d.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<Expiration>(), It.IsAny<ValueCondition>(), It.IsAny<CommandFlags>()))
            .Callback((RedisKey key, RedisValue value, Expiration expiry, ValueCondition _, CommandFlags __) => store[(string)key!] = ((string)value!, expiry))
            .ReturnsAsync(true);

        mock.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Callback((RedisKey key, CommandFlags _) =>
            {
                store.Remove((string)key!);
                deletedKeys.Add((string)key!);
            })
            .ReturnsAsync(true);

        return new FakeCache(mock, store, deletedKeys);
    }

    /// <summary>Настоящий репозиторий поверх InMemory-БД и фейкового кеша.</summary>
    public EventRepository CreateRepository(AppDbContext context, Mock<IDatabase> cache) =>
        new(context, cache.Object, NullLogger<EventRepository>.Instance, CacheSettings);

    /// <summary>Настоящий сервис с замокированными менеджером репозиториев, маппером и outbox'ом.</summary>
    public (EventService Service, Mock<IRepositoryManager> Repos, Mock<IEventRepository> Events, Mock<IOutboxService> Outbox, Mock<IMapper> Mapper) CreateService()
    {
        var repo = new Mock<IEventRepository>(MockBehavior.Strict);
        var repos = new Mock<IRepositoryManager>(MockBehavior.Strict);
        repos.SetupGet(r => r.Event).Returns(repo.Object);
        repos.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var outbox = new Mock<IOutboxService>(MockBehavior.Strict);
        outbox.Setup(o => o.EnqueueMessageAsync(It.IsAny<IIntegrationMessage>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mapper = new Mock<IMapper>(MockBehavior.Loose);

        return (new EventService(repos.Object, mapper.Object, outbox.Object), repos, repo, outbox, mapper);
    }

    /// <summary>Фабрика событий: totalSeats мест, из них sold продано.</summary>
    public static Event CreateEvent(string title, int totalSeats, int sold)
    {
        var entity = Event.Create(
            title,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            description: null,
            totalSeats: totalSeats);

        entity.TryReserveSeats(sold);
        return entity;
    }

    public static string EventKey(Guid id) => $"event:{id}";
    public const string TopKey = "events:top10";
}

/// <summary>Фейковый кеш: мок IDatabase + наблюдаемое состояние.</summary>
public sealed record FakeCache(
    Mock<IDatabase> Mock,
    Dictionary<string, (string Value, Expiration Ttl)> Store,
    List<string> DeletedKeys);
