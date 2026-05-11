using Shared.DTO;
using Contracts.Service;
using Contracts.Repository;
using AutoMapper;
using Entities.ErrorHandling.Exceptions.Booking;
using Entities.Domain.Models;
using Entities.ErrorHandling.Exceptions.Event;
using Repository;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IMapper mapper) : IBookingService
{
    public BookingDTO CreateBookingAsync(Guid eventId)
    {
        ValidateBookingFor(eventId);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };

        repositoryManager.Booking.CreateBooking(booking);

        return mapper.Map<BookingDTO>(booking);
    }

    public BookingDTO GetBookingByIdAsync(Guid bookingId)
    {
        var booking = GetBooking(bookingId);
        return mapper.Map<BookingDTO>(booking);
    }

    public IEnumerable<BookingDTO> GetPendingBookings()
    {
        var bookings = repositoryManager.Booking.GetAllPendingBookings();
        var pendingBookingDTOs = mapper.Map<IEnumerable<BookingDTO>>(bookings);
        return pendingBookingDTOs;
    }

    private Booking GetBooking(Guid bookingId)
    {
        var entity = repositoryManager.Booking.GetById(bookingId);
        if (entity is null)
            throw new BookingNotFoundException(bookingId);

        return entity;
    }

    private void ValidateBookingFor(Guid eventId)
    {
       var @event = repositoryManager.Event.GetById(eventId);
         if (@event is null)
                throw new EventNotFoundException(eventId);
    }

    public void UpdateBooking(Guid bookingId, UpdateBookingDTO updateDto)
    {
        var booking = GetBooking(bookingId);

        booking.ProcessedAt = updateDto.ProcessedAt;
        booking.Status = updateDto.Status;

        if(repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }
    }

    public void ConfirmBooking(Guid bookingId)
    {
        var updateDto = new UpdateBookingDTO(
            ProcessedAt: DateTime.UtcNow,
            Status: BookingStatus.Confirmed
        );

        UpdateBooking(bookingId, updateDto);
    }

    public void RejectBooking(Guid bookingId)
    {
        var updateDto = new UpdateBookingDTO(
            ProcessedAt: DateTime.UtcNow,
            Status: BookingStatus.Rejected
        );

        UpdateBooking(bookingId, updateDto);
    }
}
