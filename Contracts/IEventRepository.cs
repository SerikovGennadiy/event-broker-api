using Entities.Models;

namespace Contracts
{
    internal interface IEventRepository
    {
        IEnumerable<Event> GetAllEventsAsync();
        Event GetByIdAsync(Guid id);

        void CreateEvent(Event entity);
        void DeleteEvent(Event entity);
    }
}
