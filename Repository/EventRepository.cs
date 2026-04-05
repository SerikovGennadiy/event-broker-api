using Contracts.Repository;
using Entities.Domain.Models;

namespace Repository;

public class EventRepository : RepositoryBase<Event>, IEventRepository
{
    public EventRepository(RepositoryContext context) : base(context)
    { }

    public Event? GetById(Guid eventId) => FindByCondition(x => x.Id == eventId).SingleOrDefault();
    public IEnumerable<Event> GetAllEvents() => FindAll();

    public void CreateEvent(Event entity) => Create(entity);
    public void DeleteEvent(Event entity) => Delete(entity);
}
