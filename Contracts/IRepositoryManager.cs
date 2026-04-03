namespace Contracts;

public interface IRepositoryManager
{
    IEventRepository Event { get; }
    void Save();
}
