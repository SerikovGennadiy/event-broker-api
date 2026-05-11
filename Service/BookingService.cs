using Shared.DTO;
using Contracts.Service;
using Contracts.Repository;
using AutoMapper;

namespace Service;

public class BookingService(IRepositoryManager repositoryManager, IMapper mapper) : IBookingService
{
    public BookingDTO CreateBookingAsync(Guid eventId) => throw new NotImplementedException();
    public BookingDTO GetBookingByIdAsync(Guid bookingId) => throw new NotImplementedException();
}
