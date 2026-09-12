using Messaging.Bookings;
using Messaging.Events;
using System.Text.Json.Serialization;

namespace Messaging.Saga;

public interface IIntegrationMessage
{
    public Guid TraceId { get; }
    public string PartitionKey { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BookingStarted), "BookingStarted")]
[JsonDerivedType(typeof(BookingConfirmed), "BookingConfirmed")]
[JsonDerivedType(typeof(BookingRejected), "BookingRejected")]
[JsonDerivedType(typeof(BookingCancelled), "BookingCancelled")]
public interface IBookingProcessing : IIntegrationMessage
{
    public Guid BookingId { get; }
    public Guid EventId { get; }
    public Guid UserId { get; }
    string IIntegrationMessage.PartitionKey => BookingId.ToString();

}


[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EventCreatedOrUpdated), "EventCreatedOrUpdated")]
[JsonDerivedType(typeof(EventDeleted), "EventDeleted")]
public interface IEventIntegration: IIntegrationMessage
{
    public Guid EventId { get; }
    string IIntegrationMessage.PartitionKey => EventId.ToString();
}
