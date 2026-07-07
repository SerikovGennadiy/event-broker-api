using Application.Common.RequestSpecification;
using Entities.Domain.Models;

namespace Application.Contracts.Persistance;

public interface IEventRepository
{
    Task<PaginatedList<Event>> GetAllEventsAsync(EventParameters eventParameters);
    Task<Event?> GetByIdAsync(Guid id);

    void CreateEvent(Event entity);
    void DeleteEvent(Event entity);
}
