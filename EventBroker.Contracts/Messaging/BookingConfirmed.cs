using System;

namespace Contracts.Messaging
{
    public static class Topics
    {
        public const string BookingConfirmed = "booking-confirmed";
    }

    /// <summary>
    /// Событие, публикуемое сервисом Bookings при подтверждении брони.
    /// </summary>
    public sealed record BookingConfirmed(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        int Seats,
        DateTime ConfirmedAt
    );
}