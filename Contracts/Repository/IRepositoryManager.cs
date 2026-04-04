namespace Contracts.Repository;

public interface IRepositoryManager
{
    IEventRepository Event { get; }
    void Save();
}
