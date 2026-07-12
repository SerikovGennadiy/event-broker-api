using Domain.Models;

namespace Infrastructure.Persistence.Extensions;

public static class EventRepositoryExtension
{
    public static IQueryable<Event> FilterRangeEvents(this IQueryable<Event> events, DateTime? from, DateTime? to)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));

        if (from.HasValue)
            events = events.Where(e => e.StartAt >= from);

        if (to.HasValue)
            events = events.Where(e => e.EndAt <= to);

        return events;
    }

    public static IQueryable<Event> FilterTitleEvents(this IQueryable<Event> events, string? title)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));

        if (string.IsNullOrWhiteSpace(title))
            return events;

        var term = title.Trim().ToLower();
        return events.Where(e => e.Title.ToLower().Contains(term));
    }
}
