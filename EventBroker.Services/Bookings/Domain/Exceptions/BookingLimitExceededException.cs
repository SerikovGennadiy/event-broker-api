using Bookings.Domain.Exceptions.Common;

namespace Bookings.Domain.Exceptions;

public class BookingLimitExceededException : ConflictException
{
    public BookingLimitExceededException(Guid userId, int limit)
        : base($"ѕользователь (ID: {userId}) превысил лимит активных броней ({limit}).")
    { }
}