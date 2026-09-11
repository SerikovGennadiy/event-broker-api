namespace Events.Application.Contracts.Persistence;

public interface IRepositoryManager
{
    IEventRepository Event { get; }
    Task SaveAsync();
}
