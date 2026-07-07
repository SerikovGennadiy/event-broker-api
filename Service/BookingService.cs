using AutoMapper;
using Repository;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IEventService eventService, IMapper mapper) : IBookingService
{
    private static readonly SemaphoreSlim _bookigSemaphore = new(1, 1);

    public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        await _bookigSemaphore.WaitAsync(cancellationToken);

        try
        {
            Booking booking;

            booking = new Booking(eventId);
            repositoryManager.Booking.CreateBooking(booking);

            await eventService.ReserveSeats((eventId: booking.EventId, seats: 1));

            await repositoryManager.SaveAsync();

            return mapper.Map<BookingDTO>(booking);
        }
        finally
        {
            _bookigSemaphore.Release();
        }
    }

    public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await GetBookingAsync(bookingId);
        return mapper.Map<BookingDTO>(booking);
    }

    public async Task<ICollection<BookingDTO>> GetPendingBookingsAsync()
    {
        var bookings = await repositoryManager.Booking.GetAllPendingBookingsAsync();
        var pendingBookingDTOs = mapper.Map<ICollection<BookingDTO>>(bookings);
        return pendingBookingDTOs;
    }

    private async Task<Booking> GetBookingAsync(Guid bookingId)
    {
        var entity = await repositoryManager.Booking.GetByIdAsync(bookingId);
        if (entity is null)
            throw new BookingNotFoundException(bookingId);

        return entity;
    }

    public async Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var booking = await GetBookingAsync(bookingId);
        booking.Confirm();

        if (repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }

        await repositoryManager.SaveAsync();
    }

    public async Task RejectBooingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var booking = await GetBookingAsync(bookingId);
        await eventService.ReleaseSeats((eventId: booking.EventId, seats: 1));
        booking.Reject();

        if (repositoryManager.Booking is BookingRepository repo)
        {
            repo.Update(booking);
        }

        await repositoryManager.SaveAsync();
    }
}
