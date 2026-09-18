namespace Users.Application.Contracts.Persistence;

public interface IRepositoryManager
{
    IUserRepository User { get; }
    Task SaveAsync();
}
