using Bookings.Application.Common.Messaging;
using Bookings.Application.Contracts.Persistence;
using Bookings.Domain.Models;
using Bookings.Infrastructure.Persistence.Messaging.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<EventRead> EventRead => Set<EventRead>();

    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<InboxMessage> Inbox => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
