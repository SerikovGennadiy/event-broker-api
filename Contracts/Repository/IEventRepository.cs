using Entities.Domain.Models;
using Shared.RequestSpecification;

namespace Contracts.Repository;

public interface IEventRepository
{
    Task<PaginatedList<Event>> GetAllEventsAsync(EventParameters eventParameters);
    Task<Event?> GetByIdAsync(Guid id);

    void CreateEvent(Event entity);
    void DeleteEvent(Event entity);
}
