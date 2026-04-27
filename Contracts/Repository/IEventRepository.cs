using Entities.Domain.Models;
using Shared.RequestSpecification;

namespace Contracts.Repository;

public interface IEventRepository
{
    PaginatedList<Event> GetAllEvents(EventParameters eventParameters);
    Event? GetById(Guid id);

    void CreateEvent(Event entity);
    void DeleteEvent(Event entity);
}
