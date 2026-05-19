namespace Contracts.Repository;

public interface IRepositoryManager
{
    IEventRepository Event { get; }
    IBookingRepository Booking { get; }
    void Save();
}
