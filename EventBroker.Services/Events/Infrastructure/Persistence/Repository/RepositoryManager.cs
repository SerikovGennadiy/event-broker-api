using Events.Application.Contracts.Persistence;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Events.Infrastructure.Persistence.Repository;

public class RepositoryManager(AppDbContext context, IDatabase cache, ILogger<EventRepository> eventLogger) : IRepositoryManager
{
    private IEventRepository Events { get; } = new EventRepository(context, cache, eventLogger);
    public IEventRepository Event => Events;

    public async Task SaveAsync() => await context.SaveChangesAsync();
}
