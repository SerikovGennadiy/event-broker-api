using Entities.Domain.Contract;

namespace Entities.Domain.Models;

/// <summary>Модель события - мероприятия</summary>
public class Event : IdEntity, IReadOnlyEvent
{
    public Guid Id { get; set; }


    /// <summary>Наименование мероприятия</summary>
    public required string Title { get; set; }

    /// <summary>Дата и время начала мероприятия</summary>
    public required DateTime StartAt { get; set; }

    /// <summary>Дата и время завершения мероприятия</summary>
    public required DateTime EndAt { get; set; }


    /// <summary>Описание мероприятия</summary>
    public string? Description { get; set; }
}

public interface IReadOnlyEvent
{
    Guid Id { get; }
    string Title { get; }
    DateTime StartAt { get; }
    DateTime EndAt { get; }
    string? Description { get; }
}