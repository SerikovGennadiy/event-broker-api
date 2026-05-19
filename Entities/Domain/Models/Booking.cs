using Entities.Domain.Contract;
using Entities.ErrorHandling.Exceptions.Booking;

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
    public Guid Id { get; }

    /// <summary>Идентификатор мероприятия</summary>
    public Guid EventId { get; }

    /// <summary>Дата и время создания брони</summary>
    public DateTime CreatedAt { get; }

    /// <summary>Статус брони</summary>
    public BookingStatus Status { get; private set; }

    /// <summary>Дата и время обработки брони</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary> Конструктор для создания новой брони</summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="eventId">Идентификатор мероприятия</param>
    public Booking(Guid bookingId, Guid eventId)
    {
        Id = bookingId;
        EventId = eventId;
        CreatedAt = DateTime.UtcNow;
        OnPending();
    }

    /// <summary> Подтверждение брони </summary>
    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new BookingNoReverseStatus(EventId, Id, $"Нельзя подтвердить бронь в статусе {Status}");

        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary> Отклонение брони </summary>
    public void Reject()
    {
        if (Status != BookingStatus.Pending)
            throw new BookingNoReverseStatus(EventId, Id, $"Нельзя отклонить бронь в статусе {Status}");

        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public void OnPending()
    {
        if (Status == BookingStatus.Pending)
            return;

        Status = BookingStatus.Pending;
        ProcessedAt = DateTime.UtcNow;
    }
}
