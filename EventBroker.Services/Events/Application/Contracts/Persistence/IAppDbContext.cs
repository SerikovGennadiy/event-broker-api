using Events.Application.Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Events.Application.Contracts.Persistence;

public interface IAppDbContext
{
    public DatabaseFacade Database { get; }
    public DbSet<OutboxMessage> Outbox { get; }
    public DbSet<InboxMessage> Inbox { get; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
