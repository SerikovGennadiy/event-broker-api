using Entities.Domain.Contract;

namespace Entities.Domain.Models;

/// <summary>Текущий статус брони на мероприятие</summary>
public enum BookingStatus
{
    /// <summary>Бронь создана, но не подтверждена</summary>
    Pending,
    /// <summary>Бронь подтверждена</summary>
    Confirmed,
    /// <summary>Бронь отклонена</summary>
    Rejected,
}

/// <summary>Модель брони на мероприятие</summary>
public class Booking : IdEntity
{
    /// <summary>Идентификатор брони</summary>
    public Guid Id { get; set; }
    /// <summary>Идентификатор мероприятия</summary>
    public Guid EventId { get; set; }
    /// <summary>Статус брони</summary>
    public BookingStatus Status { get; set; }
    /// <summary>Дата и время создания брони</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Дата и время обработки брони</summary>
    public DateTime? ProcessedAt { get; set; }
}
