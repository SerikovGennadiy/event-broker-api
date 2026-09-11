using Messaging.Saga;

namespace Messaging.Events;

#region Статус-сообщения интеграционных событий о состоянии мероприятия (topic: event-integration)
public sealed record EventCreatedOrUpdated(Guid TraceId, Guid EventId, string Title, DateTime StartAt, DateTime EndAt) : IEventIntegration;
public sealed record EventDeleted(Guid TraceId, Guid EventId) : IEventIntegration;
#endregion

#region Статус-сообщения событий бронирования на мероприятие (topic: event-booking)
public sealed record SeatReserved(Guid TraceId, Guid BookingId, Guid EventId, Guid UserId) : IIntegarationEvent;
public sealed record SeatReleased(Guid TraceId, Guid BookingId, Guid EventId, Guid UserId) : IIntegarationEvent;
public sealed record SeatReservationFailed(Guid TraceId, Guid BookingId, Guid EventId, Guid UserId, string Error) : IIntegarationEvent;
#endregion