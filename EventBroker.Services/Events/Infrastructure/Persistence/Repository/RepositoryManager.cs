using Events.Application.Contracts.Persistence;
using Events.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Events.Infrastructure.Persistence.Repository;

public class RepositoryManager(AppDbContext context, IDatabase cache, ILogger<EventRepository> eventLogger, IOptions<RedisSettings> cacheSettings) : IRepositoryManager
{
    private IEventRepository Events { get; } = new EventRepository(context, cache, eventLogger, cacheSettings.Value);
    public IEventRepository Event => Events;

    public async Task SaveAsync() => await context.SaveChangesAsync();
}
