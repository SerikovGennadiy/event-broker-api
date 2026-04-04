using Entities.Models;
namespace Contracts.Repository;

public interface IEventRepository
{
    IEnumerable<Event> GetAllEvents();
    Event? GetById(Guid id);

    void CreateEvent(Event entity);
    void DeleteEvent(Event entity);
}
