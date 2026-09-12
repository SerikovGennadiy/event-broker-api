using Messaging.Saga;

namespace Messaging.Bookings;

#region Статус-сообщения событий бронирования
public sealed record BookingStarted(Guid TraceId,
                                    Guid BookingId,
                                    Guid EventId,
                                    Guid UserId) : IBookingProcessing
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public sealed record BookingConfirmed(Guid TraceId,
                                      Guid BookingId,
                                      Guid EventId,
                                      Guid UserId) : IBookingProcessing
{
    public DateTime ProcessedAt { get; } = DateTime.UtcNow;
}

public sealed record BookingRejected(Guid TraceId,
                                     Guid BookingId,
                                     Guid EventId,
                                     Guid UserId,
                                     string Reason) : IBookingProcessing
{
    public DateTime ProcessedAt { get; } = DateTime.UtcNow;
}

public sealed record BookingCancelled(Guid TraceId,
                                      Guid BookingId,
                                      Guid EventId,
                                      Guid UserId) : IBookingProcessing
{
    public DateTime ProcessedAt { get; } = DateTime.UtcNow;
}
#endregion

