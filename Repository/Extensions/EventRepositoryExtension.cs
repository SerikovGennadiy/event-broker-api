using Entities.Domain.Models;

namespace Repository.Extensions;

public static class EventRepositoryExtension
{
    public static IEnumerable<Event> FilterRangeEvents(this IEnumerable<Event> events, DateTime? from, DateTime? to)
    {
        if(events == null) throw new ArgumentNullException(nameof(events));

        if (from.HasValue)
            events = events.Where(e => e.StartAt >= from);

        if (to.HasValue)
            events = events.Where(e => e.EndAt <= to);

        return events;
    }

    public static IEnumerable<Event> FilterTitleEvents(this IEnumerable<Event> events, string? title)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));

        if (string.IsNullOrWhiteSpace(title))
            return events;

        var term = title.Trim().ToLower();
        return events.Where(e => e.Title.ToLower().Contains(term));
    }
}
