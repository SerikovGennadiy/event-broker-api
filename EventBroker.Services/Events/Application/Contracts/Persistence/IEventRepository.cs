using Events.Application.Common.RequestSpecification;
using Events.Domain.Models;

namespace Events.Application.Contracts.Persistence;

public interface IEventRepository
{
    Task<PaginatedList<Event>> GetAllEventsAsync(EventParameters eventParameters);
    Task<Event?> GetByIdAsync(Guid id);

    void CreateEvent(Event entity);
    void DeleteEvent(Event entity);
}
