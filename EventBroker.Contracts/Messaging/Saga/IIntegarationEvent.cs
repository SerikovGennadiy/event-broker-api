using Messaging.Bookings;
using Messaging.Events;
using System.Text.Json.Serialization;

namespace Messaging.Saga;

public interface IIntegrationEvent
{
    public Guid TraceId { get; }
    public string PartitionKey { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BookingStarted), "BookingStarted")]
[JsonDerivedType(typeof(BookingConfirmed), "BookingConfirmed")]
[JsonDerivedType(typeof(BookingRejected), "BookingRejected")]
[JsonDerivedType(typeof(BookingCancelled), "BookingCancelled")]
public interface IIntegarationEvent : IIntegrationEvent
{
    public Guid BookingId { get; }
    public Guid EventId { get; }
    public Guid UserId { get; }
    string IIntegrationEvent.PartitionKey => BookingId.ToString();

}


[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EventCreatedOrUpdated), "EventCreatedOrUpdated")]
[JsonDerivedType(typeof(EventDeleted), "EventDeleted")]
public interface IEventIntegration: IIntegrationEvent
{
    public Guid EventId { get; }
    string IIntegrationEvent.PartitionKey => EventId.ToString();
}
