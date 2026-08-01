namespace Domain.Exceptions.Booking;

public class BookingLimitExceededException : ConflictException
{
    public BookingLimitExceededException(Guid userId, int limit)
        : base($"ѕользователь (ID: {userId}) превысил лимит активных броней ({limit}).")
    { }
}