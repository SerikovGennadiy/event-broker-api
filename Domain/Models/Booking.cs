using Domain.Contracts;
using Domain.Exceptions.Booking;

namespace Domain.Models;

/// <summary>Текущий статус брони на мероприятие</summary>
public enum BookingStatus
{
    /// <summary>Бронь создана, но не подтверждена</summary>
    Pending,
    /// <summary>Бронь подтверждена</summary>
    Confirmed,
    /// <summary>Бронь отклонена</summary>
    Rejected,
    /// <summary>Бронь отменена</summary>
    Cancelled,
}

/// <summary>Модель брони на мероприятие</summary>
public class Booking : IdEntity
{
    /// <summary>Идентификатор брони</summary>
    public Guid Id { get; }

    /// <summary>Идентификатор мероприятия</summary>
    public Guid EventId { get; }

    /// <summary>Идентификатор пользователя, создавшего бронь</summary>
    public Guid UserId { get; }

    /// <summary>Дата и время создания брони</summary>
    public DateTime CreatedAt { get; }

    /// <summary>Статус брони</summary>
    public BookingStatus Status { get; private set; }

    /// <summary>Дата и время обработки брони</summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary> Навигация на событие </summary>
    public Event Event { get; set; } = null!;

    /// <summary> Навигация на пользователя </summary>
    public User? User { get; set; }

    // приватный конструктор под EFCore
    private Booking() { }

    /// <summary> Конструктор для создания новой брони</summary>
    /// <param name="eventId">Идентификатор мероприятия</param>
    /// <param name="userId">Идентификатор пользователя</param>
    public Booking(Guid eventId, Guid userId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        UserId = userId;
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

    /// <summary> Отмена брони </summary>
    public void Cancel()
    {
        // Защита от повторной отмены и некорректных переходов
        if (Status == BookingStatus.Cancelled)
            throw new BookingNoReverseStatus(EventId, Id, "Бронь уже отменена");

        if (Status == BookingStatus.Rejected)
            throw new BookingNoReverseStatus(EventId, Id, $"Нельзя отменить бронь в статусе {Status}");

        // Разрешаем отменять Pending и Confirmed
        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }

    private void OnPending()
    {
        if (Status == BookingStatus.Pending)
            return;

        Status = BookingStatus.Pending;
        ProcessedAt = DateTime.UtcNow;
    }
}