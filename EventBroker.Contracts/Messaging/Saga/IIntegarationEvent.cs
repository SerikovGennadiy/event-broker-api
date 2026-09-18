using Messaging.Bookings;
using Messaging.Events;
using System.Text.Json.Serialization;

namespace Messaging.Saga;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EventCreatedOrUpdated), "EventCreatedOrUpdated")]
[JsonDerivedType(typeof(EventDeleted), "EventDeleted")]
[JsonDerivedType(typeof(BookingStarted), "BookingStarted")]
[JsonDerivedType(typeof(BookingConfirmed), "BookingConfirmed")]
[JsonDerivedType(typeof(BookingRejected), "BookingRejected")]
[JsonDerivedType(typeof(BookingCancelled), "BookingCancelled")]
[JsonDerivedType(typeof(SeatReserved), "SeatReserved")]
[JsonDerivedType(typeof(SeatReleased), "SeatReleased")]
[JsonDerivedType(typeof(SeatReservationFailed), "SeatReservationFailed")]
public interface IIntegrationMessage
{
    public Guid TraceId { get; }
    public string PartitionKey { get; }
}


public interface IBookingProcessing : IIntegrationMessage
{
    public Guid BookingId { get; }
    public Guid EventId { get; }
    public Guid UserId { get; }
    string IIntegrationMessage.PartitionKey => BookingId.ToString();

}

public interface IEventIntegration: IIntegrationMessage
{
    public Guid EventId { get; }
    string IIntegrationMessage.PartitionKey => EventId.ToString();
}
