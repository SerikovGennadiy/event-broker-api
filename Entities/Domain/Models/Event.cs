using Entities.Domain.Contract;
using System.ComponentModel.DataAnnotations;

namespace Entities.Domain.Models;

/// <summary>Модель события - мероприятия</summary>
public class Event : IdEntity, IReadOnlyEvent
{
    public Guid Id { get; init; }


    /// <summary>Наименование мероприятия</summary>
    public required string Title { get; set; }

    /// <summary>Дата и время начала мероприятия</summary>
    public required DateTime StartAt { get; set; }

    /// <summary>Дата и время завершения мероприятия</summary>
    public required DateTime EndAt { get; set; }

    /// <summary>Описание мероприятия</summary>
    public string? Description { get; set; }

    /// <summary>Общее количество мест на мероприятии</summary>
    public required int TotalSeats { get; set; }

    /// <summary>Количество оставшихся свободных мест на мероприятии</summary>
    public int AvailableSeats { get; internal set; }

    public static Event Create(string Title, DateTime StartAt, DateTime EndAt, string? description, int totalSeats)
    {
        if (totalSeats <= 0)
            throw new ValidationException("Общее количество мест на мероприятие должно быть больше 0");

        return new Event() {
            Id = Guid.CreateVersion7(),
            Title = Title,
            StartAt = StartAt,
            EndAt = EndAt, 
            Description = description, 
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };
    }

    /// <summary>Попытка зарезервировать count мест. Если мест недостаточно — возвращает false.</summary>
    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0) return false;

        if (AvailableSeats < count)
            return false;

        AvailableSeats -= count;
        return true;
    }

    /// <summary>Освободить ранее зарезервированные места</summary>
    public void ReleaseSeats(int count = 1)
    {
        if (count <= 0) return;

        // гарантируем, что AvailableSeats не превысит TotalSeats
        var newAvailable = AvailableSeats + count;
        AvailableSeats = newAvailable > TotalSeats ? TotalSeats : newAvailable;
    }

    // конструктор, для тестирования (как корректно такое делают, чтобы не засорять код - не знаю)
    internal Event() { }
}

public interface IReadOnlyEvent
{
    Guid Id { get; }
    string Title { get; }
    DateTime StartAt { get; }
    DateTime EndAt { get; }
    string? Description { get; }
    int TotalSeats { get; }
    int AvailableSeats { get; }
}