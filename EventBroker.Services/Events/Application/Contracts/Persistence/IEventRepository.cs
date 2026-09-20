using Events.Application.Common.RequestSpecification;
using Events.Domain.Models;

namespace Events.Application.Contracts.Persistence;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<PaginatedList<Event>> GetAllEventsAsync(EventParameters eventParameters);
    Task<IReadOnlyList<Event>> GetTop10SellingEventsAsync();
    void CreateEvent(Event entity);
    void DeleteEvent(Event entity);
    Task InvalidateEventCacheAsync(Guid id);
}
