using Events.Domain.Exceptions.Common;

namespace Events.Domain.Exceptions;

public class NoAvailableSeatsException : ConflictException
{
    public NoAvailableSeatsException(Guid eventId)
        : base($"No available seats for this event (id: {eventId}")
    { }
}
