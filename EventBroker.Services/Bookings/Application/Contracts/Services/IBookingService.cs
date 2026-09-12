using Bookings.Application.Common.DTO;

namespace Bookings.Application.Contracts.Services;

public interface IBookingService
{
    /// <summary>Просмотреть бронировани</summary>
    Task<BookingDTO> GetBookingByIdAsync(Guid bookingId);

    #region Старт саг IBookingProcessing
    /// <summary>Сформировать бронирование на мероприяти</summary>
    /// <reamrks>СТАРТ САГИ: BookingCreated : IBookingProcessing</reamrks>
    Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>Отменить бронирование по инициативе пользователя</summary>
    /// <reamrks>СТАРТ САГИ BookingCancelled: IBookingProcessing</reamrks>
    Task<bool> CancelBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    #endregion

    #region Шаги саги BookingCreated: IBookingProcessing
    /// <summary></summary>
    /// <reamrks></reamrks>
    Task ConfirmBookingAsync(Guid traceId, Guid bookingId, Guid eventId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary></summary>
    /// <reamrks></reamrks>
    Task RejectBooingAsync(Guid traceId, Guid bookingId, Guid eventId, Guid userId, CancellationToken cancellationToken = default);
    #endregion
}
