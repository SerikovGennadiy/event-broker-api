using Events.Application.Common.RequestSpecification;
using Events.Application.Contracts.Persistence;
using Events.Domain.Models;
using Events.Domain.Options;
using Events.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Events.Infrastructure.Persistence.Repository;

public class EventRepository : RepositoryBase<Event>, IEventRepository
{
    private readonly IDatabase _cache;
    private readonly ILogger<EventRepository> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private const string TopEventsKey = "events:top10";
    private const int TopEventsCount = 10;

    private const int DefaultEventTtlMinutes = 5;
    private const int DefaultTopEventsTtlMinutes = 1;

    private readonly TimeSpan _eventTtl;
    private readonly TimeSpan _topEventsTtl;

    public EventRepository(AppDbContext context, IDatabase cache, ILogger<EventRepository> logger, RedisSettings cacheSettings) : base(context)
    {
        _cache = cache;
        _logger = logger;
        _eventTtl = TimeSpan.FromMinutes(cacheSettings.EventTtlMinutes > 0 ? cacheSettings.EventTtlMinutes : DefaultEventTtlMinutes);
        _topEventsTtl = TimeSpan.FromMinutes(cacheSettings.TopEventsTtlMinutes > 0 ? cacheSettings.TopEventsTtlMinutes : DefaultTopEventsTtlMinutes);
    }

    /// <summary>Cache-Aside: сначала кеш event:{id}, при промахе — БД с прогревом кеша.</summary>
    public async Task<Event?> GetByIdAsync(Guid eventId)
    {
        var key = $"event:{eventId}";

        var cached = await TryGetAsync(key);
        if (cached.HasValue)
        {
            try
            {
                var entity = JsonSerializer.Deserialize<Event>(cached!, JsonOptions);
                if (entity is not null)
                    return Attach(entity);
            }
            catch (JsonException ex)
            {
                // Битая запись в кеше — перезапросим из БД и перезапишем.
                _logger.LogWarning(ex, "Битая запись в кеше по ключу {CacheKey}, перезапрашиваем из БД", key);
            }
        }

        var fromDb = await FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
        if (fromDb is not null)
            await TrySetAsync(key, JsonSerializer.Serialize(fromDb, JsonOptions), _eventTtl);

        return fromDb;
    }

    /// <summary>Cache-Aside: топ-10 событий по проценту проданных мест, ключ events:top10.</summary>
    /// <remarks>
    /// Процент продаж: (total_seats - available_seats) / total_seats.
    /// Это рейтинг, сильный контроль целостности не нужен, устаревание в пределах TTL (1 мин).
    /// </remarks>
    public async Task<IReadOnlyList<Event>> GetTop10SellingEventsAsync()
    {
        var cached = await TryGetAsync(TopEventsKey);
        if (cached.HasValue)
        {
            try
            {
                var entities = JsonSerializer.Deserialize<List<Event>>(cached!, JsonOptions);
                if (entities is not null)
                    return entities.Select(Attach).ToList();
            }
            catch (JsonException ex)
            {
                // Битая запись в кеше — перезапросим из БД и перезапишем.
                _logger.LogWarning(ex, "Битая запись в кеше по ключу {CacheKey}, перезапрашиваем из БД", TopEventsKey);
            }
        }

        var fromDb = await FindAll()
            .Where(e => e.TotalSeats > 0)
            .OrderByDescending(e => (double)(e.TotalSeats - e.AvailableSeats) / e.TotalSeats)
            .Take(TopEventsCount)
            .ToListAsync();

        await TrySetAsync(TopEventsKey, JsonSerializer.Serialize(fromDb, JsonOptions), _topEventsTtl);

        return fromDb;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync() => await FindAll().ToListAsync();
    public async Task<PaginatedList<Event>> GetAllEventsAsync(EventParameters eventParameters)
    {
        var events = await FindAll()
                          .FilterRangeEvents(eventParameters.From, eventParameters.To)
                          .FilterTitleEvents(eventParameters.Title)
                          .ToListAsync();

        return PaginatedList<Event>.ToPagedList(events, eventParameters.Page, eventParameters.PageSize);
    }

    public void CreateEvent(Event entity) => Create(entity);
    public void DeleteEvent(Event entity) => Delete(entity);

    /// <summary>
    /// Сбрасываем кеш события после его изменения — удаляем ключ event:{id}.
    /// </summary>
    /// <remarks>
    /// Выбрано "удалить при записи", а не "обновить при записи", потому что так проще.
    /// Не закешируются данные, которые ещё не сохранились в БД:
    /// метод вызывается уже после SaveAsync, а следующий читатель сам загрузит свежие данные из базы.
    /// Ключ events:top10 тут специально не трогаем. Топ-10 — это рейтинг
    /// он может чуть-чуть устареть, это нестрашно. Он сам обновится через TTL (1 минута).
    /// Сбрасывать топ при каждом бронировании было бы слишком — лишняя работа.
    /// </remarks>
    public async Task InvalidateEventCacheAsync(Guid eventId)
    {
        try
        {
            await _cache.KeyDeleteAsync($"event:{eventId}");
        }
        catch (RedisException ex)
        {
            // Кеш недоступен — бизнес-операция уже зафиксирована в БД,
            // устаревшие записи истекут по TTL. Клиент ошибку не получает.
            _logger.LogWarning(ex, "Redis недоступен, инвалидация кеша события {EventId} пропущена", eventId);
        }
    }

    #region Cache-Aside helpers
    /// <summary>
    /// Прикрепить десериализованную из кеша сущность в ChangeTracker EF. Репозиторий понятия не имеет, что будет происходить с возвращаемыми им сущностями
    /// Поэтому перед return поместим их в отслеживаемые объекты.
    /// </summary>
    /// <remarks>
    /// Сущность из кеша detached: прикрепляем к контексту, чтобы последующие
    /// изменения (Update/Reserve/Release) отслеживались EF Core.
    /// </remarks>
    private Event Attach(Event entity)
    {
        var tracked = _context.Set<Event>().Local.FirstOrDefault(e => e.Id == entity.Id);
        if (tracked is not null)
            return tracked;

        _context.Attach(entity);
        return entity;
    }

    // Кеш — не источник истины: при недоступности Redis покажем и пойдем в БД.
    private async Task<RedisValue> TryGetAsync(string key)
    {
        try
        {
            return await _cache.StringGetAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis недоступен, чтение по ключу {CacheKey} идёт напрямую в БД", key);
            return RedisValue.Null;
        }
    }

    private async Task TrySetAsync(string key, string value, TimeSpan ttl)
    {
        try
        {
            await _cache.StringSetAsync(key, value, ttl);
        }
        catch (RedisException ex)
        {
            // Прогрев кеша, на чтение из БД не влияет. Сообщим если что не так
            _logger.LogWarning(ex, "Redis недоступен, прогрев кеша по ключу {CacheKey} пропущен", key);
        }
    }
    #endregion
}
