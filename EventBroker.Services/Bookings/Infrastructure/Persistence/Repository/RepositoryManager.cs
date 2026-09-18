using AutoMapper;
using Bookings.Application.Contracts.Persistence;
namespace Bookings.Infrastructure.Persistence.Repository;

public class RepositoryManager(AppDbContext context, IMapper mapper) : IRepositoryManager
{
    private readonly IBookingRepository Bookings = new BookingRepository(context);
    private readonly IEventReadRepository EventReads = new EventReadRepository(context, mapper);

    public IBookingRepository Booking => Bookings;
    public IEventReadRepository EventRead => EventReads;

    public async Task SaveAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);
}
