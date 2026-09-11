using Events.Application.Contracts.Persistence;

namespace Events.Infrastructure.Persistence.Repository;

public class RepositoryManager(AppDbContext context) : IRepositoryManager
{
    private IEventRepository Events { get; } = new EventRepository(context);
    public IEventRepository Event => Events;

    public async Task SaveAsync() => await context.SaveChangesAsync();
}
