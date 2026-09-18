using Bookings.Domain.Models.Contract;

namespace Bookings.Infrastructure.Persistence.Messaging.ReadModels;

/// <summary> Read - модель сущности Event </summary>
/// <remarks> Используется для валидации при формировании бронирования</remarks>
public class EventRead : IdEntity
{
    public Guid Id { get; set; }
    public DateTime StartAt { get; set; }
}
