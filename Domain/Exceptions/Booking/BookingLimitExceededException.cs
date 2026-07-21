namespace Domain.Exceptions.Booking;

public class BookingLimitExceededException : BadRequestException
{
    public BookingLimitExceededException(Guid userId, int limit)
        : base($"ѕользователь (ID: {userId}) превысил лимит активных броней ({limit}).")
    { }
}