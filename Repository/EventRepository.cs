using Contracts;
using Entities.Models;

namespace Repository;

internal class EventRepository : RepositoryBase<Event>, IEventRepository
{
    public EventRepository(RepositoryContext context) : base(context)
    { }

    public IEnumerable<Event> GetAllEvents() => FindAll();

    public void CreateEvent(Event entity) => Create(entity);
    public void DeleteEvent(Event entity) => Delete(entity);

    public Event? GetById(Guid id) => FindByCondition(x => x.Id == id).SingleOrDefault();
}
