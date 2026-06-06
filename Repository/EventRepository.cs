using Contracts.Repository;
using Entities.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestSpecification;

namespace Repository;

public class EventRepository : RepositoryBase<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context)
    { }

    public async Task<Event?> GetByIdAsync(Guid eventId) => await FindByCondition(x => x.Id == eventId).SingleOrDefaultAsync();

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
}
