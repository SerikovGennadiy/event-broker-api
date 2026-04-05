using Entities.Domain.Contract;

namespace Entities.Domain.Models;

/// <summary>Модель события - мероприятия</summary>
public class Event : IdEntity
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
