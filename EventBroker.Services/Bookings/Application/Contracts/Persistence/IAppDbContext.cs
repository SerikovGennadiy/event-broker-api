using Bookings.Application.Common.Messaging;
using Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bookings.Application.Contracts.Persistence;
public interface IAppDbContext
{
    DatabaseFacade Database { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<OutboxMessage> Outbox { get; }
    DbSet<InboxMessage> Inbox { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
