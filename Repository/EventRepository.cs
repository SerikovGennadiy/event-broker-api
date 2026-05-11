using Contracts.Repository;
using Entities.Domain.Models;
using Repository.Extensions;
using Shared.RequestSpecification;

namespace Repository;

public class EventRepository : RepositoryBase<Event>, IEventRepository
{
    public EventRepository(RepositoryContext context) : base(context)
    { }

    public Event? GetById(Guid eventId) => FindByCondition(x => x.Id == eventId).SingleOrDefault();

    public IEnumerable<Event> GetAllEvents() => FindAll();
    public PaginatedList<Event> GetAllEvents(EventParameters eventParameters)
    {
       var events = FindAll()
                   .FilterRangeEvents(eventParameters.From, eventParameters.To)
                   .FilterTitleEvents(eventParameters.Title)
                   .ToList();

        return PaginatedList<Event>.ToPagedList(events, eventParameters.Page, eventParameters.PageSize);

    }

    public void CreateEvent(Event entity) => Create(entity);
    public void DeleteEvent(Event entity) => Delete(entity);
}
