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
    public IEnumerable<Event> GetAllEvents(EventParameters eventParameters) =>
        // TODO все методы до фактического запроса готовят его через IQueryable? пока через IEnumerable
                                                FindAll()
                                               .FilterRangeEvents(eventParameters.From, eventParameters.To)
                                               .FilterTitleEvents(eventParameters.Title)
                                               .ToList();

    public void CreateEvent(Event entity) => Create(entity);
    public void DeleteEvent(Event entity) => Delete(entity);
}
