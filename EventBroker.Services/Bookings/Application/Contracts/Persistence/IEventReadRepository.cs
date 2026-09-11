using Bookings.Application.Common.DTO;

namespace Bookings.Application.Contracts.Persistence;

public interface IEventReadRepository
{
    // Возвращает null, если событие не найдено
    Task<EventReadDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(EventReadDTO eventReadDTO, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
