namespace Application.Contracts.Persistance;

public interface IRepositoryManager
{
    IEventRepository Event { get; }
    IBookingRepository Booking { get; }
    IUserRepository User { get; }
    Task SaveAsync();
}
